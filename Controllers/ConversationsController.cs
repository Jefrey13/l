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
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversations;

        public ConversationsController(IConversationService conversations)
        {
            _conversations = conversations;
        }

        [HttpGet(Name = "GetAllConversations")]
        [SwaggerOperation(Summary = "List all conversations")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var list = await _conversations.GetAllAsync(ct);
            return Ok(new ApiResponse<IEnumerable<ConversationDto>>(list, "All conversations retrieved."));
        }

        [HttpGet("pending", Name = "GetPendingConversations")]
        [SwaggerOperation(Summary = "List all conversations waiting for human agent")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPending(CancellationToken ct = default)
        {
            var list = await _conversations.GetPendingAsync(ct);
            return Ok(new ApiResponse<IEnumerable<ConversationDto>>(list, "Pending conversations retrieved."));
        }

        [HttpGet("{id}", Name = "GetConversationById")]
        [SwaggerOperation(Summary = "Get conversation details by ID")]
        [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _conversations.GetByIdAsync(id, ct);
            if (dto == null)
                return NotFound(new ApiResponse<object>(null, "Conversation not found."));
            return Ok(new ApiResponse<ConversationDto>(dto, "Conversation retrieved."));
        }

        [HttpPost(Name = "StartConversation")]
        [SwaggerOperation(Summary = "Start a new conversation")]
        [ProducesResponseType(typeof(ApiResponse<ConversationDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Start(
            [FromBody] StartConversationRequest req,
            CancellationToken ct = default)
        {
            var dto = await _conversations.StartAsync(req, ct);
            return CreatedAtRoute(
                "GetConversationById",
                new { id = dto.ConversationId },
                new ApiResponse<ConversationDto>(dto, "Conversation started.")
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
            await _conversations.AssignAgentAsync(id, agentUserId, status , ct);
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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConversationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUserRole(CancellationToken ct = default)
        {
            var jwtToken = HttpContext.Request
                             .Headers["Authorization"]
                             .ToString()
                             .Split(' ')[1];

            var list = await _conversations.GetConversationByRole(jwtToken, ct);
            return Ok(new ApiResponse<IEnumerable<ConversationDto>>(list,
                       "Conversations retrieved by user role."));
        }
    }
}