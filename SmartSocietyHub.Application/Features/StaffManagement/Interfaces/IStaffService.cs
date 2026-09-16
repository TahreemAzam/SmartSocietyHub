using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartSocietyHub.Application.Features.StaffManagement.DTOs;

namespace SmartSocietyHub.Application.Features.StaffManagement.Interfaces
{
    public interface IStaffService
    {
        Task<CreateMaintenanceStaffResponse> CreateMaintenanceStaffAsync(
            CreateMaintenanceStaffRequest request);

        Task<List<MaintenanceStaffResponse>> GetAllMaintenanceStaffAsync();
    }
}
