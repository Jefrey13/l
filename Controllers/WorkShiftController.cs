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
    public class WorkShiftController : ControllerBase
    {
        private readonly IWorkShiftService _workShiftService;
        public WorkShiftController(IWorkShiftService workShiftService)
        {
            _workShiftService = workShiftService;
        }

        [HttpGet("{id}", Name = "GetWorkShiftById")]
        [SwaggerOperation(Summary = "Workshift get by id.")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var response = await _workShiftService.GetByIdAsync(id, ct);

            if (response == null) return NotFound(new ApiResponse<WorkShiftResponseDto>(null, "Id invalido", false));

            return Ok(new ApiResponse<WorkShiftResponseDto>(response, "Success", true, null));
        }

        [HttpGet(Name = "GetAllWorkShift")]
        [SwaggerOperation(Summary = "Get all workshift")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var response = await _workShiftService.GetAllAsync(@params, ct);
            if (response.Items == null) return NotFound(new ApiResponse<WorkShiftResponseDto>(null, "Recurso no encontrados", false));

            return Ok(new ApiResponse<PagedResponse<WorkShiftResponseDto>>(response, "Success", true, null));
        }
        [HttpPost(Name = "CreateWorkShift")]
        [SwaggerOperation(Summary = "Create new work shift")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] WorkShiftRequestDto request, CancellationToken ct= default)
        {
            if (request == null) return BadRequest(new ApiResponse<WorkShiftResponseDto>(null, "Los datos del turno no pueden ser null", false));

            var jwtToken = await GetTokenAsync();
            
            var response = await _workShiftService.CreateAsync(request, jwtToken, ct);
            if (response == null) return NotFound(new ApiResponse<WorkShiftResponseDto>(null, "Datos no creados", false));

            return Ok(new ApiResponse<WorkShiftResponseDto>(response, "success", true, null));
        }

        [HttpPut(Name = "UpdateWorkShift")]
        [SwaggerOperation(Summary = "Update new work shift")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] WorkShiftRequestDto request, CancellationToken ct = default)
        {
            if (id <= 0 || request == null) return BadRequest(new ApiResponse<WorkShiftResponseDto>(null, "Los datos no pueden ser null", false));

            var jwtToken = await GetTokenAsync();

            var response = await _workShiftService.UpdateAsync(id, request, jwtToken, ct);

            if (response == null) return NotFound(new ApiResponse<WorkShiftResponseDto>(null, "Datos no actualizados", false));

            return Ok(new ApiResponse<WorkShiftResponseDto>(response, "success", true));
        }

        [HttpPatch(Name = "ToggleWorkShift")]
        [SwaggerOperation(Summary = "Change state of an work shift")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Toggle([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return BadRequest(new ApiResponse<WorkShiftResponseDto>(null, "Id invalido", false));

            var jwtToken = await GetTokenAsync();
            var response = await _workShiftService.ToggleAsync(id, jwtToken, ct);

            if (response == null) return NotFound(new ApiResponse<WorkShiftResponseDto>(null, "Datos no actualizados", false));

            return Ok(new ApiResponse<WorkShiftResponseDto>(response, "success", true));
        }
        private async Task<string> GetTokenAsync()
        {
            return HttpContext.Request.Headers["Authorization"].ToString().Split(' ').Last();
        }
    }
}