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

        private string GetJwt()
        {
            var header = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Token missing or invalid");
            return header.Split(' ')[1];
        }

        [HttpGet("{id}", Name = "GetWorkShiftById")]
        [SwaggerOperation(Summary = "Retrieve a specific work shift by its ID")]
        [ProducesResponseType(typeof(ApiResponse<WorkShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "Invalid ID", false));

            try
            {
                var dto = await _workShiftService.GetByIdAsync(id, ct);
                return Ok(new ApiResponse<WorkShiftResponseDto>(dto, "Work shift retrieved successfully", true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve a paginated list of all work shifts")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<WorkShiftResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _workShiftService.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<WorkShiftResponseDto>>(paged, "Work shifts retrieved successfully", true));
        }

        [HttpGet("by-date")]
        [SwaggerOperation(Summary = "Retrieve all work shifts active on a specific date")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WorkShiftResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByDate([FromQuery] DateOnly date, CancellationToken ct = default)
        {
            if (date == default)
                return BadRequest(new ApiResponse<object>(null, "Invalid date", false));

            var list = await _workShiftService.GetByDateAsync(date, ct);
            return Ok(new ApiResponse<IEnumerable<WorkShiftResponseDto>>(list, $"Work shifts for {date:yyyy-MM-dd} retrieved successfully", true));
        }

        [HttpGet("count-by-date")]
        [SwaggerOperation(Summary = "Count the number of active work shift assignments on a specific date")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveCount([FromQuery] DateOnly date, CancellationToken ct = default)
        {
            if (date == default)
                return BadRequest(new ApiResponse<object>(null, "Invalid date", false));

            var count = await _workShiftService.GetActiveAssignmentsCountAsync(date, ct);
            return Ok(new ApiResponse<int>(count, $"Active assignments count for {date:yyyy-MM-dd}: {count}", true));
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new work shift assignment")]
        [ProducesResponseType(typeof(ApiResponse<WorkShiftResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] WorkShiftRequestDto request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(null, "Invalid request data", false));

            string token;
            try { token = GetJwt(); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new ApiResponse<object>(null, ex.Message, false)); }

            try
            {
                var dto = await _workShiftService.CreateAsync(request, token, ct);
                return CreatedAtRoute("GetWorkShiftById", new { id = dto.Id }, new ApiResponse<WorkShiftResponseDto>(dto, "Work shift created successfully", true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update an existing work shift assignment")]
        [ProducesResponseType(typeof(ApiResponse<WorkShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] WorkShiftRequestDto request, CancellationToken ct = default)
        {
            if (id <= 0 || !ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(null, "Invalid request data", false));

            string token;
            try { token = GetJwt(); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new ApiResponse<object>(null, ex.Message, false)); }

            try
            {
                var dto = await _workShiftService.UpdateAsync(id, request, token, ct);
                return Ok(new ApiResponse<WorkShiftResponseDto>(dto, "Work shift updated successfully", true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>(null, ex.Message, false));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpPatch("{id}")]
        [SwaggerOperation(Summary = "Toggle active/inactive status of a work shift assignment")]
        [ProducesResponseType(typeof(ApiResponse<WorkShiftResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Toggle(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "Invalid ID", false));

            string token;
            try { token = GetJwt(); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new ApiResponse<object>(null, ex.Message, false)); }

            try
            {
                var dto = await _workShiftService.ToggleAsync(id, token, ct);
                return Ok(new ApiResponse<WorkShiftResponseDto>(dto, "Work shift status toggled successfully", true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }
    }
}