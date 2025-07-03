using System.Collections.Generic;
using System.Threading;
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
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ContactLogsController : ControllerBase
    {
        private readonly IContactLogService _contactLogService;

        public ContactLogsController(IContactLogService contactLogService)
        {
            _contactLogService = contactLogService;
        }

        [HttpGet(Name = "GetAllContactLogs")]
        [SwaggerOperation(Summary = "Retrieve paged list of contacts")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ContactLogResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _contactLogService.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<ContactLogResponseDto>>(paged, "Contacts retrieved."));
        }

        [HttpGet("pending-approval", Name = "GetPendingApprovalContactLogs")]
        [SwaggerOperation(Summary = "Retrieve contacts pending approval")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContactLogResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingApprovalAsync(CancellationToken ct = default)
        {
            var list = await _contactLogService.GetPendingApprovalAsync(ct);
            return Ok(new ApiResponse<IEnumerable<ContactLogResponseDto>>(list, "Pending contacts retrieved."));
        }

        [HttpGet("{id}", Name = "GetContactLogById")]
        [SwaggerOperation(Summary = "Retrieve a contact by ID")]
        [ProducesResponseType(typeof(ApiResponse<ContactLogResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _contactLogService.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "Contact not found."));
            return Ok(new ApiResponse<ContactLogResponseDto>(dto, "Contact retrieved."));
        }

        [HttpPut("verifyContact/{id}", Name ="Verify new contact.")]
        [SwaggerOperation(Summary = "Verify contact detail by the admin.")]
        [ProducesResponseType(typeof(ApiResponse<ContactLogResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ContactLogResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyAsync([FromRoute] int id, CancellationToken ct = default)
        {
            if (id <= 0) return NotFound(new ApiResponse<ContactLogResponseDto>(null, "Id is required", false));

            var jwtToken = HttpContext.Request
                             .Headers["Authorization"]
                             .ToString()

                             .Split(' ')[1];
             await _contactLogService.VerifyAsync(id, jwtToken, ct);

            return Ok(new ApiResponse<ContactLogResponseDto>(null, "Success. Activation contact success", true, null));
        }

        [HttpGet("contact/{phone}", Name = "GetContactLogByPhone")]
        [SwaggerOperation(Summary = "Retrieve a contact by phone number")]
        [ProducesResponseType(typeof(ApiResponse<ContactLogResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPhoneAsync([FromRoute] string phone, CancellationToken ct = default)
        {
            var dto = await _contactLogService.GetByPhoneAsync(phone, ct);

            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "Contact not found."));
            return Ok(new ApiResponse<ContactLogResponseDto>(dto, "Contact retrieved."));
        }

        [HttpPost(Name = "CreateContactLog")]
        [SwaggerOperation(Summary = "Create a new contact")]
        [ProducesResponseType(typeof(ApiResponse<ContactLogResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateContactLogRequestDto requestDto, CancellationToken ct = default)
        {
            var dto = await _contactLogService.CreateAsync(requestDto, ct);
            return CreatedAtRoute("GetContactLogById", new { id = dto.Id },
                new ApiResponse<ContactLogResponseDto>(dto, "Contact created."));
        }

        [HttpPut("{id}", Name = "UpdateContactLog")]
        [SwaggerOperation(Summary = "Update an existing contact")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] int id,
            [FromBody] UpdateContactLogRequestDto requestDto,
            CancellationToken ct = default)
        {
            if (id != requestDto.Id)
                return BadRequest(new ApiResponse<object>(null, "Mismatched contact ID."));
             
            if(requestDto == null)
                return BadRequest(new ApiResponse<object>(null, "Request body is required."));

            var res = await _contactLogService.UpdateAsync(requestDto, ct);

            if (res is null)
                return NotFound(new ApiResponse<object>(null, "Contact not found."));

            return Ok(new ApiResponse<ContactLogResponseDto>(res, "Contact updated successfully.", true, null));
        }

        [HttpPatch("{id}", Name = "ToggleContactLog")]
        [SwaggerOperation(Summary = "Toggle (isActive)")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleAsync([FromRoute] int id, CancellationToken ct = default)
        {
            if(id <= 0)
                return NotFound(new ApiResponse<object>(null, "Id is required."));  

           var response =  await _contactLogService.ToggleAsync(id, ct);
            if(response is null)
                return NotFound(new ApiResponse<object>(null, "Contact not found."));

            return Ok(new ApiResponse<ContactLogResponseDto>(response, "Mensaje actualizado con exito", true, null));
        }
    }
}