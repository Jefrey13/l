using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WhatsappBusiness.CloudApi.Messages.Requests;

namespace CustomerService.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private IContactLogService _contactLogService;

        public ContactController(IContactLogService contactLogService)
        {
            _contactLogService = contactLogService;
        }

        [HttpGet(Name = "GetAllContactAsync")]
        [SwaggerOperation(Summary = "Retrive lis of contact")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContactLogResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContactLogResponseDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContactLogResponseDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _contactLogService.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<ContactLogResponseDto>>(paged, "Contactos obtenidos con exito"));
        }

        [HttpGet("{id}", Name= "GetById")]
        [SwaggerOperation(Summary = "Get contact by id")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByIdAsync([FromRoute]  int id, CancellationToken ct = default)
        {
            var dto = await _contactLogService.GetByIdAsync(id, ct);
            if(dto is null)
                return NotFound(new ApiResponse<ContactLogResponseDto>(null, "Contacto no encontrado"));
            return Ok(new ApiResponse<ContactLogResponseDto>(dto, "Contacto recuperado con exito."));
        }

        [HttpGet("contact/{phone}", Name = "GetByPhone")]
        [SwaggerOperation(summary: "Get contact by phone number")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetByPhoneAsync([FromRoute] string phone, CancellationToken ct = default)
        {
            var dto = await _contactLogService.GetByPhone(phone, ct);
            if (dto is null)
                return NotFound(new ApiResponse<ContactLogResponseDto>(null, "Contacto no encontrado"));
            return Ok(new ApiResponse<ContactLogResponseDto>(dto, "Contacto recuperado con exito."));
        }

        [HttpPost(Name = "CreateContact")]
        [SwaggerOperation(summary: "Create contact")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateContact([FromBody] CreateContactLogRequestDto requestDto, 
            CancellationToken cancellation)
        {
            var dto = await _contactLogService.CreateAsync(requestDto, cancellation);

            if (dto is null) 
                return NotFound(new ApiResponse<ContactLogResponseDto>(null, "Usuario no encontrado"));

            return Ok(new ApiResponse<ContactLogResponseDto>(dto, "Usuario creado con exito"));
        }

        [HttpPut(Name = "UpdateContact")]
        [SwaggerOperation(summary: "Updating contact properties")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(
            [FromRoute] int id,
            [FromBody] UpdateContactLogRequestDto requestDto, 
            CancellationToken ct)
        {
            if (id != requestDto.Id)
                return BadRequest(new ApiResponse<object>(null, "Mismatched role Id"));

            await _contactLogService.UpdateAsync(requestDto, ct);
            return NoContent();
        }

        [HttpDelete(Name ="DeleteContact")]
        [SwaggerOperation(summary: "Desactive contact")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            await _contactLogService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}