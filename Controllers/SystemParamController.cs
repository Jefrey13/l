using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SystemParamController : ControllerBase
    {
        private readonly ISystemParamService _systemParamService;

        public SystemParamController(ISystemParamService systemParamService)
        {
            _systemParamService = systemParamService
                ?? throw new ArgumentNullException(nameof(systemParamService));
        }

        [HttpGet("{id:int}", Name = "GetSystemParamById")]
        [SwaggerOperation(Summary = "Obtener un SystemParam por ID")]
        [ProducesResponseType(typeof(ApiResponse<SystemParamResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "El ID debe ser mayor que cero." });
            }

            var responseDto = await _systemParamService.GetByIdAsync(id);
            // Si no existe, el servicio lanza KeyNotFoundException y cae al middleware de errores.
            return Ok(new ApiResponse<SystemParamResponseDto>(
                responseDto,
                "Parámetro obtenido con éxito.",
                true,
                null));
        }

        [HttpGet("name/{name}", Name = "GetSystemParamByName")]
        [SwaggerOperation(Summary = "Obtener un SystemParam por nombre")]
        [ProducesResponseType(typeof(ApiResponse<SystemParamResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "El nombre no puede ser nulo o vacío." });
            }

            var responseDto = await _systemParamService.GetByNameAsync(name);
            return Ok(new ApiResponse<SystemParamResponseDto>(
                responseDto,
                "Parámetro obtenido con éxito.",
                true,
                null));
        }

        [HttpDelete("{id:int}", Name = "DeleteSystemParam")]
        [SwaggerOperation(Summary = "Alternar el estado IsActive de un SystemParam")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "El ID debe ser mayor que cero." });
            }

            await _systemParamService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost(Name = "CreateSystemParam")]
        [SwaggerOperation(Summary = "Crear un nuevo SystemParam")]
        [ProducesResponseType(typeof(ApiResponse<SystemParamResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                return BadRequest(new { Message = "El cuerpo de la petición no puede ser nulo." });
            }

            var createdParam = await _systemParamService.CreateAsync(systemParam);
            return CreatedAtRoute(
                "GetSystemParamById",
                new { id = createdParam.Id },
                new ApiResponse<SystemParamResponseDto>(
                    createdParam,
                    "Parámetro creado con éxito.",
                    true,
                    null));
        }

        [HttpPut("{id:int}", Name = "UpdateSystemParam")]
        [SwaggerOperation(Summary = "Actualizar un SystemParam existente")]
        [ProducesResponseType(typeof(ApiResponse<SystemParamResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                return BadRequest(new { Message = "El cuerpo de la petición no puede ser nulo." });
            }

            if (id != systemParam.Id)
            {
                return BadRequest(new { Message = "El ID de la ruta debe coincidir con el ID del payload." });
            }

            var updatedParam = await _systemParamService.UpdateAsync(systemParam);
            return Ok(new ApiResponse<SystemParamResponseDto>(
                updatedParam,
                "Parámetro actualizado con éxito.",
                true,
                null));
        }

        [HttpGet(Name = "GetAllSystemParams")]
        [SwaggerOperation(Summary = "Obtener todos los SystemParams")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemParamResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllAsync()
        {
            var allParams = await _systemParamService.GetAllAsync();
            if (allParams == null || !allParams.Any())
            {
                return NotFound(new { Message = "No se encontraron parámetros." });
            }

            return Ok(new ApiResponse<IEnumerable<SystemParamResponseDto>>(
                allParams,
                "Parámetros obtenidos con éxito.",
                true,
                null));
        }
    }
}