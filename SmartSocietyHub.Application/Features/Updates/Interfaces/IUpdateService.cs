using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSocietyHub.Application.Features.Updates.DTOs;

namespace SmartSocietyHub.Application.Features.Updates.Interfaces
{
    public interface IUpdateService
    {
        // ============================================================
        // UPDATE MANAGEMENT - ADMIN
        // ============================================================

        Task<UpdateResponse> CreateUpdateAsync(
            CreateUpdateRequest request);

        Task<List<UpdateResponse>> GetAllUpdatesAsync();

        Task<UpdateResponse?> GetUpdateByIdAsync(
            Guid updateId);

        Task<UpdateResponse?> UpdateUpdateAsync(
            Guid updateId,
            UpdateUpdateRequest request);

        Task<bool> DeleteUpdateAsync(
            Guid updateId);

        Task<UpdateResponse?> PublishUpdateAsync(
            Guid updateId);

        Task<UpdateResponse?> UnpublishUpdateAsync(
            Guid updateId);


        // ============================================================
        // PUBLISHED UPDATES - ALL NON-ADMIN USERS
        // ============================================================

        Task<List<UpdateResponse>> GetPublishedUpdatesAsync();

        Task<UpdateResponse?> GetPublishedUpdateByIdAsync(
            Guid updateId);

        Task<List<UpdateResponse>> GetUpcomingEventsAsync();


        // ============================================================
        // USER-SPECIFIC UPDATE READ STATUS
        // ============================================================

        Task<int> GetUnseenUpdateCountAsync(
            Guid userId);

        Task<List<UpdateResponse>> GetRecentUnseenUpdatesAsync(
            Guid userId);

        Task<bool> MarkUpdateAsSeenAsync(
            Guid userId,
            Guid updateId);
    }
}