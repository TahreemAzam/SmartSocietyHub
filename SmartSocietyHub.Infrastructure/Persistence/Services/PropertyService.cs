using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartSocietyHub.Application.Features.PropertyManagement.DTOs;
using SmartSocietyHub.Application.Features.PropertyManagement.Interfaces;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly ApplicationDbContext _context;

        public PropertyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PropertyResponse> CreateAsync(
            CreatePropertyRequest request)
        {
            var property = new Property
            {
                Id = Guid.NewGuid(),
                HouseNumber = request.HouseNumber,
                Block = request.Block,
                Status = request.Status
            };

            _context.Properties.Add(property);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException postgresException &&
                      postgresException.SqlState == "23505")
            {
                throw new InvalidOperationException(
                    "A property with this house number and block already exists.");
            }

            return new PropertyResponse
            {
                Id = property.Id,
                HouseNumber = property.HouseNumber,
                Block = property.Block,
                Status = property.Status
            };
        }

        public async Task<List<PropertyResponse>> GetAllAsync()
        {
            return await _context.Properties
                .Select(property => new PropertyResponse
                {
                    Id = property.Id,
                    HouseNumber = property.HouseNumber,
                    Block = property.Block,
                    Status = property.Status
                })
                .ToListAsync();
        }

        public async Task<PropertyResponse?> GetByIdAsync(Guid id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return null;
            }

            return new PropertyResponse
            {
                Id = property.Id,
                HouseNumber = property.HouseNumber,
                Block = property.Block,
                Status = property.Status
            };
        }

        public async Task<PropertyResponse?> UpdateAsync(
            Guid id,
            CreatePropertyRequest request)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return null;
            }

            property.HouseNumber = request.HouseNumber;
            property.Block = request.Block;
            property.Status = request.Status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException postgresException &&
                      postgresException.SqlState == "23505")
            {
                throw new InvalidOperationException(
                    "A property with this house number and block already exists.");
            }

            return new PropertyResponse
            {
                Id = property.Id,
                HouseNumber = property.HouseNumber,
                Block = property.Block,
                Status = property.Status
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return false;
            }

            _context.Properties.Remove(property);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException postgresException &&
                      postgresException.SqlState == "23503")
            {
                throw new InvalidOperationException(
                    "This property cannot be deleted because it is linked to a resident or complaint record.");
            }

            return true;
        }
    }
}