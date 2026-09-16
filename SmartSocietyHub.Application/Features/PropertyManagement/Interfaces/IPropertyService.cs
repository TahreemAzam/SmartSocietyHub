using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSocietyHub.Application.Features.PropertyManagement.DTOs;

namespace SmartSocietyHub.Application.Features.PropertyManagement.Interfaces
{
    public interface IPropertyService
    {
        Task<PropertyResponse> CreateAsync(CreatePropertyRequest request);

        Task<List<PropertyResponse>> GetAllAsync();

        Task<PropertyResponse?> GetByIdAsync(Guid id);

        Task<PropertyResponse?> UpdateAsync(
            Guid id,
            CreatePropertyRequest request);

        Task<bool> DeleteAsync(Guid id);
    }
}