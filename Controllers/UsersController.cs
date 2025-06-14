using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly IPresenceService _presence;
        private readonly INicDatetime _nicDatetime;
        private readonly IWebHostEnvironment _env;

        public UsersController(
            IUserService users,
            IPresenceService presence,
            INicDatetime nicDatetime,
            IWebHostEnvironment env)
        {
            _users = users;
            _presence = presence;
            _nicDatetime = nicDatetime;
            _env = env;
        }

        [HttpGet(Name = "GetAllUsers")]
        [SwaggerOperation(Summary = "Retrieve paged list of users")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<UserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationParams @params,
            CancellationToken ct = default)
        {
            var paged = await _users.GetAllAsync(@params, ct);
            return Ok(new ApiResponse<PagedResponse<UserResponseDto>>(paged, "Users retrieved."));
        }

        [HttpGet("{id}", Name = "GetUserById")]
        [SwaggerOperation(Summary = "Retrieve a user by ID")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            var dto = await _users.GetByIdAsync(id, ct);
            if (dto == null)
                return NotFound(new ApiResponse<object>(null, "User not found."));
            return Ok(new ApiResponse<UserResponseDto>(dto, "User retrieved."));
        }

        [HttpPost(Name = "CreateUser")]
        [SwaggerOperation(Summary = "Create a new user (incluyendo sus roles y foto)")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromForm] CreateUserRequest req,
            IFormFile? imageFile,
            CancellationToken ct = default)
        {
            // 1) Guardar el archivo si viene
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = $"{DateTime.UtcNow.Ticks}{Path.GetExtension(imageFile.FileName)}";
                var mediaPath = Path.Combine(_env.WebRootPath, "media");
                Directory.CreateDirectory(mediaPath);
                var filePath = Path.Combine(mediaPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream, ct);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                req.ImageUrl = $"{baseUrl}/media/{fileName}";
            }

            // 2) Llamar al servicio
            var dto = await _users.CreateAsync(req, ct);

            // 3) Devolver 201 con Location
            return CreatedAtRoute(
                "GetUserById",
                new { id = dto.UserId },
                new ApiResponse<UserResponseDto>(dto, "User created."));
        }

        [HttpPut("{id}", Name = "UpdateUser")]
        [SwaggerOperation(Summary = "Update an existing user (y sus roles y foto)")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            [FromRoute] int id,
            [FromForm] UpdateUserRequest req,
            IFormFile? imageFile,
            CancellationToken ct = default)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(null, "Mismatched user ID."));

            req.UserId = id;

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = $"{DateTime.UtcNow.Ticks}{Path.GetExtension(imageFile.FileName)}";
                var mediaPath = Path.Combine(_env.WebRootPath, "media");
                Directory.CreateDirectory(mediaPath);
                var filePath = Path.Combine(mediaPath, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream, ct);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                req.ImageUrl = $"{baseUrl}/media/{fileName}";
            }

            await _users.UpdateAsync(req, ct);
            return NoContent();
        }

        [HttpPatch("{id}", Name = "ActivationUser")]
        [SwaggerOperation(Summary = "Activate/deactivate a user")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activation(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            await _users.ActivationAsync(id, ct);
            return NoContent();
        }

        [HttpGet("agents", Name = "GetAgentsByRole")]
        [SwaggerOperation(Summary = "Lista de usuarios filtrado por rol")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AgentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgents(
            [FromQuery] string role,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(role))
                return BadRequest(ApiResponse<object>.Fail("Debe especificar el parámetro 'role' para filtrar."));

            var agents = await _users.GetByRoleAsync(role, ct);
            return Ok(new ApiResponse<IEnumerable<AgentDto>>(agents, $"Usuarios con rol '{role}' obtenidos."));
        }

        [HttpGet("{id}/status", Name = "GetStatus")]
        [SwaggerOperation(Summary = "Estado de conexión del usuario")]
        [ProducesResponseType(typeof(ApiResponse<PresenceResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatus(
            [FromRoute] int id)
        {
            var last = await _presence.GetLastOnlineAsync(id);
            var isOnline = last.HasValue;

            var dto = new PresenceResponseDto
            {
                LastOnline = last,
                IsOnline = isOnline
            };

            return Ok(new ApiResponse<PresenceResponseDto>(dto, "Status retrieved."));
        }
    }
}