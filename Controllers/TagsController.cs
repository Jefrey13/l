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
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tags;

        public TagsController(ITagService tags)
        {
            _tags = tags;
        }

        [HttpGet(Name = "GetAllTags")]
        [SwaggerOperation(Summary = "Retrieve paged list of tags")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<TagDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var paged = await _tags.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<TagDto>>(paged, "Tags retrieved."));
        }

        [HttpGet("{id}", Name = "GetTagById")]
        [SwaggerOperation(Summary = "Retrieve a tag by ID")]
        [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var dto = await _tags.GetByIdAsync(id, ct);
            if (dto is null)
                return NotFound(new ApiResponse<object>(null, "Tag not found."));
            return Ok(new ApiResponse<TagDto>(dto, "Tag retrieved."));
        }

        [HttpPost(Name = "CreateTag")]
        [SwaggerOperation(Summary = "Create a new tag")]
        [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] TagDto req, CancellationToken ct = default)
        {
            var dto = await _tags.CreateAsync(req, ct);
            return CreatedAtRoute("GetTagById", new { id = dto.TagId },
                new ApiResponse<TagDto>(dto, "Tag created."));
        }

        [HttpPut("{id}", Name = "UpdateTag")]
        [SwaggerOperation(Summary = "Update an existing tag")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TagDto req, CancellationToken ct = default)
        {
            if (id != req.TagId)
                return BadRequest(new ApiResponse<object>(null, "Mismatched tag ID."));

            await _tags.UpdateAsync(req, ct);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteTag")]
        [SwaggerOperation(Summary = "Delete a tag by ID")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            await _tags.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}