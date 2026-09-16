using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSocietyHub.Application.Features.PropertyManagement.DTOs;
using SmartSocietyHub.Application.Features.PropertyManagement.Interfaces;

namespace SmartSocietyHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreatePropertyRequest request)
        {
            try
            {
                var property = await _propertyService.CreateAsync(request);

                return Ok(property);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var properties = await _propertyService.GetAllAsync();

            return Ok(properties);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var property = await _propertyService.GetByIdAsync(id);

            if (property == null)
            {
                return NotFound(new
                {
                    message = "Property not found."
                });
            }

            return Ok(property);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            CreatePropertyRequest request)
        {
            try
            {
                var property = await _propertyService.UpdateAsync(id, request);

                if (property == null)
                {
                    return NotFound(new
                    {
                        message = "Property not found."
                    });
                }

                return Ok(property);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _propertyService.DeleteAsync(id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        message = "Property not found."
                    });
                }

                return Ok(new
                {
                    message = "Property deleted successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}