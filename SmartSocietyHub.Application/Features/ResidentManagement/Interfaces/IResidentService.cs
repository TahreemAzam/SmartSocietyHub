using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartSocietyHub.Application.Features.ResidentManagement.DTOs;

namespace SmartSocietyHub.Application.Features.ResidentManagement.Interfaces
{
    public interface IResidentService
    {
        Task<CreateResidentResponse> CreateAsync(
            CreateResidentRequest request);

        Task<List<ResidentResponse>> GetAllAsync();

        Task<ResidentResponse?> GetByIdAsync(Guid id);

        Task<ResidentResponse?> GetByApplicationUserIdAsync(
            Guid applicationUserId);

        Task<ResidentResponse?> UpdateAsync(
            Guid id,
            CreateResidentRequest request);

        Task<bool> DeleteAsync(Guid id);
    }
}