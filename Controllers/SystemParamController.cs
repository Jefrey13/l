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

        private string GetJwt()
        {
            var header = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Token missing or invalid");
            return header.Split(' ')[1];
        }

        [HttpPatch("{id:int}", Name = "ToggleSystemParamStatus")]
        [SwaggerOperation(Summary = "Alternar el estado IsActive de un SystemParam")]
        [ProducesResponseType(typeof(ApiResponse<SystemParamResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ToggleStatusAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "El ID debe ser mayor que cero.", false));

            string token;
            try
            {
                token = GetJwt();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<object>(null, ex.Message, false));
            }

            try
            {
                var dto = await _systemParamService.ToggleAsync(id, token, ct);
                return Ok(new ApiResponse<SystemParamResponseDto>(
                    dto,
                    "Estado del parámetro alternado con éxito.",
                    true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
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

        [HttpGet]
        [SwaggerOperation(Summary = "Obtener una lista paginada de SystemParams")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<SystemParamResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _systemParamService.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<SystemParamResponseDto>>(
                paged,
                "Parámetros del sistema obtenidos correctamente",
                true
            ));
        }
    }
}