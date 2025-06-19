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
    public class OpeningHourController : ControllerBase
    {
        private readonly IOpeningHourService _openingHourService;

        public OpeningHourController(IOpeningHourService openingHourService)
        {
            _openingHourService = openingHourService;
        }

        [HttpGet(Name = "GetAllOpeningHour")]
        [SwaggerOperation(Summary = "Retrieve paged list of opening hours")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<OpeningHourResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var result = await _openingHourService.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<OpeningHourResponseDto>>(result, "Opening hours retrieved successfully", true));
        }

        [HttpGet("{id}", Name = "GetOpeningHourById")]
        [SwaggerOperation(Summary = "Retrieve opening hour by ID")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await _openingHourService.GetByIdAsync(id, ct);
            return result != null
                ? Ok(new ApiResponse<OpeningHourResponseDto>(result, "Opening hour found", true))
                : NotFound(new ApiResponse<object>(null, "Opening hour not found", false));
        }

        [HttpPost(Name = "CreateOpeningHour")]
        [SwaggerOperation(Summary = "Create a new opening hour")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] OpeningHourRequestDto request, CancellationToken ct = default)
        {
            var token = GetToken();
            var result = await _openingHourService.CreateAsync(request, token, ct);
            return CreatedAtRoute("GetOpeningHourById", new { id = result.Id }, new ApiResponse<OpeningHourResponseDto>(result, "Opening hour created", true));
        }

        [HttpPut("{id}", Name = "UpdateOpeningHour")]
        [SwaggerOperation(Summary = "Update an existing opening hour")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] OpeningHourRequestDto request, CancellationToken ct = default)
        {
            if (id <= 0 || request == null)
                return BadRequest(new ApiResponse<object>(null, "Invalid request parameters", false));

            var token = GetToken();
            var result = await _openingHourService.UpdateAsync(id, request, token, ct);

            return result != null
                ? Ok(new ApiResponse<OpeningHourResponseDto>(result, "Opening hour updated", true))
                : NotFound(new ApiResponse<object>(null, "Opening hour not found", false));
        }

        [HttpPatch("{id}", Name = "ToggleOpeningHour")]
        [SwaggerOperation(Summary = "Toggle the active status of an opening hour")]
        [ProducesResponseType(typeof(ApiResponse<OpeningHourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Toggle([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "Invalid ID", false));

            var token = GetToken();
            var result = await _openingHourService.ToggleAsync(id, token, ct);

            return result != null
                ? Ok(new ApiResponse<OpeningHourResponseDto>(result, "Opening hour status toggled", true))
                : NotFound(new ApiResponse<object>(null, "Opening hour not found", false));
        }

        private string GetToken()
        {
            return HttpContext.Request.Headers["Authorization"].ToString().Split(' ').Last();
        }
    }
}