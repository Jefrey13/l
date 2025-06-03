using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemParamController : ControllerBase
    {
        private readonly ISystemParamService _systemParamService;

        public SystemParamController(ISystemParamService systemParamService)
        {
            _systemParamService = systemParamService ?? throw new ArgumentNullException(nameof(systemParamService));
        }

        [HttpGet("{id:int}", Name = "GetSystemParamById")]
        [SwaggerOperation(Summary = "Get all system params by id")]
        [ProducesResponseType(typeof(SystemParamResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            id = id <= 0 ? throw new ArgumentException("ID deve ser mayor a 0", nameof(id)) : id;
            //if (!await _systemParamService.ExistsAsync(id))
            //{
            //    return NotFound(new { Message = "System parameter not found." });
            //}
            var responseDto = await _systemParamService.GetByIdAsync(id);
            if (responseDto == null)
            {
                return NotFound(new { Message = "System parameter no puede ser null." });
            }
            return Ok(new ApiResponse<SystemParamResponseDto>(responseDto, "Parametro obenido con éxito.", true, null));
        }

        [HttpGet("name/{name}", Name = "GetSystemParamByName")]
        [SwaggerOperation(summary: "Get all system params by name")]
        [ProducesResponseType(typeof(SystemParamResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { Message = "Nombre no puede ser null." });
            }
            var responseDto = await _systemParamService.GetByNameAsync(name);

            if (responseDto == null) return NotFound(new { Message = "System parameter no puede ser null." });

            return Ok(new ApiResponse<SystemParamResponseDto>(responseDto, "Parametros obenido con éxito.", true, null));
        }

        [HttpDelete("{id:int}", Name = "DeleteSystemParam")]
        [SwaggerOperation(summary: "Update system param state (toggle)")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            id = id <= 0 ? throw new ArgumentException("ID deve ser mayor a 0.", nameof(id)) : id;
            //if (!await _systemParamService.ExistsAsync(id))
            //{
            //    return NotFound(new { Message = "System parameter not found." });
            //}
            await _systemParamService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost(Name = "CreateSystemParam")]
        [SwaggerOperation(summary: "Create new system param async")]
        [ProducesResponseType(typeof(SystemParamResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                return BadRequest(new { Message = "System parameter no puede ser null." });
            }
            var createdParam = await _systemParamService.CreateAsync(systemParam);

            return CreatedAtRoute("GetSystemParamById", new { id = createdParam.Id },
                new ApiResponse<SystemParamResponseDto>(createdParam, "Parametro creado con éxito.", true, null));
        }

        [HttpPut(Name = "UpdateSystemParam")]
        [SwaggerOperation(summary:"Update system param by id")]
        [ProducesResponseType(typeof(SystemParamResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromBody] SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                return BadRequest(new { Message = "System parameter no puede ser null." });
            }
            var updatedParam = await _systemParamService.UpdateAsync(systemParam);
            return Ok(new ApiResponse<SystemParamResponseDto>(updatedParam, "Parametro actualizado con éxito.", true, null));
        }

        [HttpGet("/", Name = "GetAllSystemParams")]
        [SwaggerOperation(summary:"Get all async system params")]
        [ProducesResponseType(typeof(IEnumerable<SystemParamResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllAsync()
        {
            var allParams = await _systemParamService.GetAllAsync();
            if (allParams == null || !allParams.Any())
            {
                return NotFound(new { Message = "No se encontro recuersos" });
            }
            return Ok(new ApiResponse<IEnumerable<SystemParamResponseDto>>(allParams, "Parametros obtenidos con éxito.", true, null));
        }
    }
}
