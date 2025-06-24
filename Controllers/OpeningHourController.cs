using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OpeningHourController : ControllerBase
    {
        private readonly IOpeningHourService _service;

        public OpeningHourController(IOpeningHourService service)
        {
            _service = service;
        }

        private string GetJwt()
        {
            var header = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Token missing or invalid");
            return header.Split(' ')[1];
        }

        [HttpGet("{id}", Name = "GetOpeningHourById")]
        [SwaggerOperation(Summary = "Retrieve a specific opening hour by its ID")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "Invalid ID", false));

            try
            {
                var dto = await _service.GetByIdAsync(id, ct);
                return Ok(new ApiResponse<OpeningHourResponseDto>(dto, "Opening hour retrieved successfully", true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve a paginated list of opening hours")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<OpeningHourResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _service.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<OpeningHourResponseDto>>(paged, "Opening hours retrieved successfully", true));
        }

        [HttpGet("effective-schedule")]
        [SwaggerOperation(Summary = "Retrieve all opening rules effective on a specific date")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OpeningHourResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetEffectiveSchedule([FromQuery] DateOnly date, CancellationToken ct = default)
        {
            if (date == default)
                return BadRequest(new ApiResponse<object>(null, "Invalid date", false));

            var list = await _service.GetEffectiveScheduleAsync(date, ct);
            return Ok(new ApiResponse<IEnumerable<OpeningHourResponseDto>>(list, $"Schedule for {date:yyyy-MM-dd} retrieved successfully", true));
        }

        [HttpGet("is-holiday")]
        [SwaggerOperation(Summary = "Check if a specific date is a holiday")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsHoliday([FromQuery] DateOnly date, CancellationToken ct = default)
        {
            if (date == default)
                return BadRequest(new ApiResponse<object>(null, "Invalid date", false));

            var result = await _service.IsHolidayAsync(date, ct);
            return Ok(new ApiResponse<bool>(result, result ? "Date is a holiday" : "Date is not a holiday", true));
        }

        [HttpGet("is-out-of-hours")]
        [SwaggerOperation(Summary = "Check if a specific instant is outside opening hours")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsOutOfHours([FromQuery] DateTime instant, CancellationToken ct = default)
        {
            if (instant == default)
                return BadRequest(new ApiResponse<object>(null, "Invalid timestamp", false));

            var result = await _service.IsOutOfOpeningHourAsync(instant, ct);
            return Ok(new ApiResponse<bool>(result, result ? "Outside opening hours" : "Within opening hours", true));
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new opening hour rule")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] OpeningHourRequestDto request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(null, "Invalid request data", false));

            string token;
            try { token = GetJwt(); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new ApiResponse<object>(null, ex.Message, false)); }

            try
            {
                var dto = await _service.CreateAsync(request, token, ct);
                return CreatedAtRoute("GetOpeningHourById", new { id = dto.Id }, new ApiResponse<OpeningHourResponseDto>(dto, "Opening hour created successfully", true));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>(null, ex.Message, false));
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update an existing opening hour rule")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] OpeningHourRequestDto request, CancellationToken ct = default)
        {
            if (id <= 0 || !ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(null, "Invalid request data", false));

            string token;
            try { token = GetJwt(); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new ApiResponse<object>(null, ex.Message, false)); }

            try
            {
                var dto = await _service.UpdateAsync(id, request, token, ct);
                return Ok(new ApiResponse<OpeningHourResponseDto>(dto, "Opening hour updated successfully", true));
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
        [SwaggerOperation(Summary = "Toggle active/inactive status of an opening hour rule")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
                var dto = await _service.ToggleAsync(id, token, ct);
                return Ok(new ApiResponse<OpeningHourResponseDto>(dto, "Opening hour status toggled successfully", true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message, false));
            }
        }
    }
}