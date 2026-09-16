using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SmartSocietyHub.Application.Features.Updates.DTOs;
using SmartSocietyHub.Application.Features.Updates.Interfaces;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly ApplicationDbContext _context;

        public UpdateService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // UPDATE MANAGEMENT - ADMIN
        // ============================================================

        public async Task<UpdateResponse> CreateUpdateAsync(
            CreateUpdateRequest request)
        {
            ValidateBasicFields(
                request.Title,
                request.Description);

            UpdateType updateType = ParseUpdateType(request.Type);

            ValidateEventFields(
                updateType,
                request.EventDate,
                request.StartTime,
                request.EndTime,
                request.Location);

            var update = new Update
            {
                Id = Guid.NewGuid(),

                Title = request.Title.Trim(),

                Description = request.Description.Trim(),

                Type = updateType,

                // Every new update starts as a draft.
                IsPublished = false,

                PublishedAt = null,

                EventDate = updateType == UpdateType.Event
                    ? request.EventDate!.Value.Date
                    : null,

                StartTime = updateType == UpdateType.Event
                    ? request.StartTime
                    : null,

                EndTime = updateType == UpdateType.Event
                    ? request.EndTime
                    : null,

                Location = updateType == UpdateType.Event
                    ? request.Location!.Trim()
                    : null,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = null
            };

            _context.Updates.Add(update);

            await _context.SaveChangesAsync();

            return MapToResponse(update);
        }

        public async Task<List<UpdateResponse>> GetAllUpdatesAsync()
        {
            var updates = await _context.Updates
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return updates
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<UpdateResponse?> GetUpdateByIdAsync(
            Guid updateId)
        {
            var update = await _context.Updates
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
            {
                return null;
            }

            return MapToResponse(update);
        }

        public async Task<UpdateResponse?> UpdateUpdateAsync(
            Guid updateId,
            UpdateUpdateRequest request)
        {
            ValidateBasicFields(
                request.Title,
                request.Description);

            var update = await _context.Updates
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
            {
                return null;
            }

            UpdateType updateType = ParseUpdateType(request.Type);

            ValidateEventFields(
                updateType,
                request.EventDate,
                request.StartTime,
                request.EndTime,
                request.Location);

            update.Title = request.Title.Trim();

            update.Description = request.Description.Trim();

            update.Type = updateType;

            // --------------------------------------------------------
            // Event-specific fields
            // --------------------------------------------------------

            if (updateType == UpdateType.Event)
            {
                update.EventDate = request.EventDate!.Value.Date;

                update.StartTime = request.StartTime;

                update.EndTime = request.EndTime;

                update.Location = request.Location!.Trim();
            }
            else
            {
                // Announcements do not have event information.
                update.EventDate = null;

                update.StartTime = null;

                update.EndTime = null;

                update.Location = null;
            }

            // IMPORTANT:
            // Editing an already-published update does NOT unpublish it.
            // IsPublished and PublishedAt remain unchanged.

            update.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(update);
        }

        public async Task<bool> DeleteUpdateAsync(
            Guid updateId)
        {
            var update = await _context.Updates
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
            {
                return false;
            }

            _context.Updates.Remove(update);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UpdateResponse?> PublishUpdateAsync(
            Guid updateId)
        {
            var update = await _context.Updates
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
            {
                return null;
            }

            if (update.IsPublished)
            {
                return MapToResponse(update);
            }

            update.IsPublished = true;

            update.PublishedAt = DateTime.UtcNow;

            update.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(update);
        }

        public async Task<UpdateResponse?> UnpublishUpdateAsync(
            Guid updateId)
        {
            var update = await _context.Updates
                .FirstOrDefaultAsync(u => u.Id == updateId);

            if (update == null)
            {
                return null;
            }

            if (!update.IsPublished)
            {
                return MapToResponse(update);
            }

            update.IsPublished = false;

            update.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(update);
        }


        // ============================================================
        // PUBLISHED UPDATES - ALL NON-ADMIN USERS
        // ============================================================

        public async Task<List<UpdateResponse>> GetPublishedUpdatesAsync()
        {
            var updates = await _context.Updates
                .AsNoTracking()
                .Where(u => u.IsPublished)
                .OrderByDescending(u => u.PublishedAt)
                .ToListAsync();

            return updates
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<UpdateResponse?> GetPublishedUpdateByIdAsync(
            Guid updateId)
        {
            var update = await _context.Updates
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Id == updateId &&
                    u.IsPublished);

            if (update == null)
            {
                return null;
            }

            return MapToResponse(update);
        }

        public async Task<List<UpdateResponse>> GetUpcomingEventsAsync()
        {
            DateTime today = DateTime.UtcNow.Date;

            var updates = await _context.Updates
                .AsNoTracking()
                .Where(u =>
                    u.IsPublished &&
                    u.Type == UpdateType.Event &&
                    u.EventDate != null &&
                    u.EventDate >= today)
                .OrderBy(u => u.EventDate)
                .ThenBy(u => u.StartTime)
                .ToListAsync();

            return updates
                .Select(MapToResponse)
                .ToList();
        }


        // ============================================================
        // USER-SPECIFIC UPDATE READ STATUS
        // ============================================================

        public async Task<int> GetUnseenUpdateCountAsync(
            Guid userId)
        {
            int count = await _context.Updates
                .AsNoTracking()
                .Where(u =>
                    u.IsPublished &&
                    !_context.UpdateReadStatuses.Any(r =>
                        r.UpdateId == u.Id &&
                        r.UserId == userId))
                .CountAsync();

            return count;
        }

        public async Task<List<UpdateResponse>> GetRecentUnseenUpdatesAsync(
            Guid userId)
        {
            DateTime recentCutoff =
                DateTime.UtcNow.AddHours(-24);

            var updates = await _context.Updates
                .AsNoTracking()
                .Where(u =>
                    u.IsPublished &&
                    u.PublishedAt != null &&
                    u.PublishedAt >= recentCutoff &&
                    !_context.UpdateReadStatuses.Any(r =>
                        r.UpdateId == u.Id &&
                        r.UserId == userId))
                .OrderByDescending(u => u.PublishedAt)
                .ToListAsync();

            return updates
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<bool> MarkUpdateAsSeenAsync(
            Guid userId,
            Guid updateId)
        {
            var updateExists = await _context.Updates
                .AsNoTracking()
                .AnyAsync(u =>
                    u.Id == updateId &&
                    u.IsPublished);

            if (!updateExists)
            {
                return false;
            }

            var alreadySeen = await _context.UpdateReadStatuses
                .AnyAsync(r =>
                    r.UpdateId == updateId &&
                    r.UserId == userId);

            // Mark-as-seen is intentionally idempotent.
            if (alreadySeen)
            {
                return true;
            }

            var readStatus = new UpdateReadStatus
            {
                Id = Guid.NewGuid(),

                UpdateId = updateId,

                UserId = userId,

                SeenAt = DateTime.UtcNow
            };

            _context.UpdateReadStatuses.Add(readStatus);

            await _context.SaveChangesAsync();

            return true;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private static void ValidateBasicFields(
            string title,
            string description)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Title is required.");
            }

            if (title.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "Title cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(
                    "Description is required.");
            }

            if (description.Trim().Length > 2000)
            {
                throw new ArgumentException(
                    "Description cannot exceed 2000 characters.");
            }
        }

        private static UpdateType ParseUpdateType(
            string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException(
                    "Type is required.");
            }

            if (type.Equals(
                    "Announcement",
                    StringComparison.OrdinalIgnoreCase))
            {
                return UpdateType.Announcement;
            }

            if (type.Equals(
                    "Event",
                    StringComparison.OrdinalIgnoreCase))
            {
                return UpdateType.Event;
            }

            throw new ArgumentException(
                "Type must be either Announcement or Event.");
        }

        private static void ValidateEventFields(
            UpdateType updateType,
            DateTime? eventDate,
            TimeSpan? startTime,
            TimeSpan? endTime,
            string? location)
        {
            // --------------------------------------------------------
            // Announcement
            // --------------------------------------------------------

            if (updateType == UpdateType.Announcement)
            {
                return;
            }

            // --------------------------------------------------------
            // Event
            // --------------------------------------------------------

            if (!eventDate.HasValue)
            {
                throw new ArgumentException(
                    "Event date is required for an Event.");
            }

            if (!startTime.HasValue)
            {
                throw new ArgumentException(
                    "Start time is required for an Event.");
            }

            if (!endTime.HasValue)
            {
                throw new ArgumentException(
                    "End time is required for an Event.");
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(
                    "Location is required for an Event.");
            }

            if (location.Trim().Length > 200)
            {
                throw new ArgumentException(
                    "Location cannot exceed 200 characters.");
            }

            if (eventDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException(
                    "An event cannot be created or updated with a past date.");
            }

            if (endTime.Value <= startTime.Value)
            {
                throw new ArgumentException(
                    "Event end time must be later than start time.");
            }
        }


        // ============================================================
        // MAPPING
        // ============================================================

        private static UpdateResponse MapToResponse(
            Update update)
        {
            return new UpdateResponse
            {
                Id = update.Id,

                Title = update.Title,

                Description = update.Description,

                Type = update.Type.ToString(),

                IsPublished = update.IsPublished,

                PublishedAt = update.PublishedAt,

                EventDate = update.EventDate,

                StartTime = update.StartTime,

                EndTime = update.EndTime,

                Location = update.Location,

                CreatedAt = update.CreatedAt,

                UpdatedAt = update.UpdatedAt
            };
        }
    }
}