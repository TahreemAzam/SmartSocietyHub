using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SmartSocietyHub.Application.Features.ComplaintManagement.DTOs;

namespace SmartSocietyHub.Application.Features.ComplaintManagement.Interfaces
{
    public interface IComplaintService
    {
        // Resident creates a complaint
        Task<ComplaintResponse> CreateAsync(
            Guid applicationUserId,
            CreateComplaintRequest request);

        // Resident gets their own complaints
        Task<List<ComplaintResponse>> GetMyComplaintsAsync(
            Guid applicationUserId);

        // Admin gets all complaints
        Task<List<ComplaintResponse>> GetAllAsync();

        // Maintenance Staff gets complaints assigned to them
        Task<List<ComplaintResponse>> GetAssignedToMeAsync(
            Guid maintenanceStaffUserId);

        // Get one complaint by ID
        Task<ComplaintResponse?> GetByIdAsync(Guid id);

        // Admin assigns Maintenance Staff
        Task<ComplaintResponse?> AssignAsync(
            Guid complaintId,
            AssignComplaintRequest request);

        // Maintenance Staff updates complaint status
        Task<ComplaintResponse?> UpdateStatusAsync(
            Guid complaintId,
            Guid maintenanceStaffUserId,
            UpdateComplaintStatusRequest request);

        // Resident accepts resolution or reopens complaint
        Task<ComplaintResponse?> ResidentActionAsync(
            Guid complaintId,
            Guid applicationUserId,
            ResidentComplaintActionRequest request);
    }
}