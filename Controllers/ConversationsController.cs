using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.RequestDtos.ConversationDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Implementations;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversations;
        public ConversationsController(IConversationService conversations)
        {
            _conversations = conversations;
        }

        [HttpGet(Name = "GetAllConversations")]
        [SwaggerOperation(Summary = "List all conversations")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var list = await _conversations.GetAllAsync(ct);
            return Ok(new ApiResponse<IEnumerable<ConversationResponseDto>>(list, "All conversations retrieved."));
        }

        [HttpGet("pending", Name = "GetPendingConversations")]
        [SwaggerOperation(Summary = "List all conversations waiting for human agent")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPending(CancellationToken ct = default)
        {
            var list = await _conversations.GetPendingAsync(ct);
            return Ok(new ApiResponse<IEnumerable<ConversationResponseDto>>(list, "Pending conversations retrieved."));
        }

        [HttpGet("{id}", Name = "GetConversationById")]
        [SwaggerOperation(Summary = "Get conversation details by ID")]
        [ProducesResponseType(typeof(ApiResponse<ConversationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _conversations.GetByIdAsync(id, ct);
            if (dto == null)
                return NotFound(new ApiResponse<object>(null, "Conversation not found."));
            return Ok(new ApiResponse<ConversationResponseDto>(dto, "Conversation retrieved."));
        }

        [HttpPost(Name = "StartConversation")]
        [SwaggerOperation(Summary = "Start a new conversation")]
        [ProducesResponseType(typeof(ApiResponse<ConversationResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Start(
            [FromBody] StartConversationRequest req,
            CancellationToken ct = default)
        {
            var dto = await _conversations.StartAsync(req, ct);
            return CreatedAtRoute(
                "GetConversationById",
                new { id = dto.ConversationId },
                new ApiResponse<ConversationResponseDto>(dto, "Conversation started.")
            );
        }

        [HttpPut("{id}/assign", Name = "AssignAgent")]
        [SwaggerOperation(Summary = "Assign an agent and update status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Assign(
            [FromRoute] int id,
            [FromQuery] int agentUserId,
            [FromQuery] string status,
            CancellationToken ct = default)
        {
            var jwtToken = HttpContext.Request
                             .Headers["Authorization"]
                             .ToString()
                             .Split(' ')[1];

            await _conversations.AssignAgentAsync(id, agentUserId, status , jwtToken, ct);
            return NoContent();
        }

        [HttpPut("tags/{id}", Name = "UpdateTag")]
        [SwaggerOperation(Summary = "Update conversation's tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] List<string> request, CancellationToken ct = default)
        {
            await _conversations.UpdateTags(id, request, ct);
            return NoContent();
        }

        [HttpPut("{id}/close", Name = "CloseConversation")]
        [SwaggerOperation(Summary = "Close an active conversation")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Close(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            await _conversations.CloseAsync(id, ct);
            return NoContent();
        }

        [HttpGet("{id}/assigned-count")]
        [SwaggerOperation(Summary = "Get count of assigned conversations for an agent")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssignedCount([FromRoute] int id, CancellationToken ct = default)
        {
            var count = await _conversations.GetAssignedCountAsync(id, ct);
            return Ok(count);
        }

        [HttpGet("getByRole", Name = "GetByUserRole")]
        [SwaggerOperation(Summary = "Get conversation details by UserRole")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUserRole(CancellationToken ct = default)
        {
            var jwtToken = HttpContext.Request
                             .Headers["Authorization"]
                             .ToString()
                             .Split(' ')[1];

            var list = await _conversations.GetConversationByRole(jwtToken, ct);
            return Ok(new ApiResponse<IEnumerable<ConversationResponseDto>>(list,
                       "Conversations retrieved by user role."));
        }

        [HttpGet("{contactId}/history", Name = "GetHistoryByContact")]
        [SwaggerOperation("GetHistoryByContact")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory([FromRoute] int contactId, CancellationToken ct)
        {
            var history = await _conversations.GetHistoryByContactAsync(contactId, ct);
            return Ok(new ApiResponse<IEnumerable<ConversationHistoryDto>>(history, "Historial obtenido."));
        }

        [HttpPost("{contactId}/summary", Name = "SummarizeAllByContact")]
        [SwaggerOperation("SummarizeAllByContact")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SummarizeAllByContact([FromRoute] int contactId, CancellationToken ct)
        {
            var summary = await _conversations.SummarizeAllByContactAsync(contactId, ct);
            return Ok(new ApiResponse<string>(summary, "Resumen generado."));
        }

        [HttpPost("{id}/respond", Name = "RespondAssignment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Respond(
        [FromRoute] int id,
        [FromBody] RespondAssignmentRequest req,   // crea este DTO con { bool Accepted; string? Comment; }
        CancellationToken ct = default)
        {
            await _conversations.RespondAssignmentAsync(id, req.Accepted, req.Comment, ct);
            return NoContent();
        }

        [HttpPost("{id}/force", Name = "ForceAssignment")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Force(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            await _conversations.ForceAssignAsync(id, ct);
            return NoContent();
        }
    }
}