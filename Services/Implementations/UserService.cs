using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User = CustomerService.API.Models.User;

namespace CustomerService.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IPresenceService _presence;
        private readonly IEmailService _email;
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenService _jwt;
        private readonly IAuthTokenRepository _tokens;
        private readonly INicDatetime _nicDatetime;

        public UserService(
            IUnitOfWork uow,
            IPasswordHasher hasher,
            IPresenceService presence,
            IEmailService email,
            IOptions<JwtSettings> jwtOptions,
            ITokenService jwt,
            IAuthTokenRepository tokens,
            INicDatetime nicDatetime)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _presence = presence ?? throw new ArgumentNullException(nameof(presence));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _jwt = jwt ?? throw new ArgumentNullException(nameof(jwt));
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _nicDatetime = nicDatetime;
        }

        public async Task<PagedResponse<UserResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.Users.GetAll();
            var paged = await PagedList<User>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var users = paged.ToList();
            var userIds = users.Select(u => u.UserId).ToList();

            var conversationCounts = await _uow.Conversations
                .GetAll()
                .Where(c => c.AssignedAgentId.HasValue && userIds.Contains(c.AssignedAgentId.Value))
                .GroupBy(c => c.AssignedAgentId.Value)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellation);

            var lastOnlines = await _presence.GetLastOnlineAsync(userIds, cancellation);

            var userRoles = await _uow.UserRoles
                .GetAll()
                .Where(ur => userIds.Contains(ur.UserId))
                .Include(ur => ur.Role)
                .ToListAsync(cancellation);

            var rolesByUser = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => new RoleResponseDto
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.RoleName,
                        Description = ur.Role.Description
                    }).ToList()
                );

            var dtos = users.Select(u =>
            {
                lastOnlines.TryGetValue(u.UserId, out var last);
                var count = conversationCounts.GetValueOrDefault(u.UserId, 0);
                return new UserResponseDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CompanyId = u.CompanyId,
                    Phone = u.Phone,
                    Identifier = u.Identifier,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    ImageUrl = u.ImageUrl,
                    LastOnline = last,
                    IsOnline = last.HasValue && (DateTime.UtcNow - last.Value).TotalMinutes < 5,
                    ClientType = count switch { 0 => "New", > 0 and <= 5 => "Frequent", _ => "VIP" },
                    Roles = rolesByUser.GetValueOrDefault(u.UserId, new List<RoleResponseDto>())
                };
            }).ToList();

            return new PagedResponse<UserResponseDto>(dtos, paged.MetaData);
        }

        public async Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            try
            {

                if (id <= 0) throw new ArgumentException("Invalid user ID.", nameof(id));

                var user = await _uow.Users.GetByIdAsync(id, cancellation)
                           ?? throw new KeyNotFoundException("User not found.");

                var lastDict = await _presence.GetLastOnlineAsync(new[] { id }, cancellation);
                lastDict.TryGetValue(id, out var last);

                var conversationCount = await _uow.Conversations
                    .GetAll()
                    .CountAsync(c => c.AssignedAgentId == id, cancellation);

                var roles = await _uow.UserRoles.GetRolesByUserIdAsync(id, cancellation);
                var rolesDto = roles.Select(ur => new RoleResponseDto
                {
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.RoleName,
                    Description = ur.Role.Description
                });

                return new UserResponseDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CompanyId = user.CompanyId,
                    Phone = user.Phone,
                    Identifier = user.Identifier,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    ImageUrl = user.ImageUrl,
                    LastOnline = last,
                    IsOnline = last.HasValue && (DateTime.UtcNow - last.Value).TotalMinutes < 5,
                    ClientType = conversationCount switch { 0 => "New", > 0 and <= 5 => "Frequent", _ => "VIP" },
                    Roles = rolesDto
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new UserResponseDto();
            }
        }

        public async Task<UserResponseDto> CreateAsync(CreateUserRequest request, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Password is required.", nameof(request.Password));
            if (await _uow.Users.ExistsAsync(u => u.Email == request.Email, cancellation))
                throw new InvalidOperationException("Email already in use.");

            try
            {

                var hash = _hasher.Hash(request.Password);
                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = Encoding.UTF8.GetBytes(hash),
                    IsActive = false,
                    CompanyId = request.CompanyId,
                    Phone = request.Phone,
                    Identifier = request.Identifier,
                    CreatedAt = await _nicDatetime.GetNicDatetime(),
                    ImageUrl = request.ImageUrl
                };

                await _uow.Users.AddAsync(user, cancellation);

                try
                {
                    await _uow.SaveChangesAsync(cancellation);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error", e.Message);
                }

                var currentRoles = new List<RoleResponseDto>();
                if (request.RoleIds.Any())
                {
                    var validRoleIds = request.RoleIds.Distinct();
                    foreach (var roleId in validRoleIds)
                    {
                        if (!await _uow.Roles.ExistsAsync(r => r.RoleId == roleId, cancellation))
                            throw new KeyNotFoundException($"RoleId {roleId} does not exist.");

                        await _uow.UserRoles.AddAsync(new UserRole
                        {
                            UserId = user.UserId,
                            RoleId = roleId,
                            AssignedAt = DateTime.UtcNow
                        }, cancellation);

                        var role = await _uow.Roles.GetByIdAsync(roleId, cancellation);
                        currentRoles.Add(new RoleResponseDto
                        {
                            RoleId = role.RoleId,
                            RoleName = role.RoleName,
                            Description = role.Description
                        });
                    }
                    await _uow.SaveChangesAsync(cancellation);
                }

                var verifyToken = _jwt.GenerateRefreshToken();
                var authToken = new AuthToken
                {
                    UserId = user.UserId,
                    Token = verifyToken,
                    TokenType = TokenType.Verification.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    CreatedAt = DateTime.UtcNow
                };

                await _tokens.AddAsync(authToken, cancellation);
                await _uow.SaveChangesAsync(cancellation);

                var verificationLink = $"http://localhost:5173/verify-account?token={Uri.EscapeDataString(verifyToken)}";
                var emailBody = $@"<div><p>Welcome <strong>{user.FullName}</strong>,</p><p>Please verify your account <a href='{verificationLink}'>here</a>.</p></div>";
                await _email.SendAsync(user.Email, "Verify your account", emailBody);

                return new UserResponseDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    CompanyId = user.CompanyId,
                    Phone = user.Phone,
                    Identifier = user.Identifier,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    ImageUrl = user.ImageUrl,
                    LastOnline = null,
                    IsOnline = false,
                    ClientType = "New",
                    Roles = currentRoles
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new UserResponseDto();
            }
        }

        public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellation = default)
        {
            try
            {
                if (request is null) throw new ArgumentException("Error. No se ha envidado datos del usuario");

                var user = await _uow.Users.GetByIdAsync(request.UserId, cancellation)
                           ?? throw new KeyNotFoundException("User not found.");

                var localTime = await _nicDatetime.GetNicDatetime();
                user.FullName = request.FullName;
                user.IsActive = request.IsActive;
                user.CompanyId = request.CompanyId;
                user.Phone = request.Phone;
                user.Identifier = request.Identifier;
                user.UpdatedAt = localTime;

                //if (!string.IsNullOrWhiteSpace(request.NewPassword))
                //    user.PasswordHash = Encoding.UTF8.GetBytes(_hasher.Hash(request.NewPassword));

                var existingRoleIds = (await _uow.UserRoles.GetRolesByUserIdAsync(user.UserId, cancellation))
                    .Select(ur => ur.RoleId).ToList();

                var rolesToRemove = existingRoleIds.Except(request.RoleIds).ToList();
                var rolesToAdd = request.RoleIds.Except(existingRoleIds).ToList();

                foreach (var rid in rolesToRemove)
                {
                    var ur = await _uow.UserRoles.GetAll()
                        .FirstAsync(x => x.UserId == user.UserId && x.RoleId == rid, cancellation);
                    _uow.UserRoles.Remove(ur);
                }

                foreach (var rid in rolesToAdd)
                    await _uow.UserRoles.AddAsync(new UserRole { UserId = user.UserId, RoleId = rid, AssignedAt = DateTime.UtcNow }, cancellation);

                _uow.Users.Update(user);
                await _uow.SaveChangesAsync(cancellation);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Local: ", ex);
            }
        }

        public async Task ActivationAsync(int userId, CancellationToken cancellation = default)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID.", nameof(userId));

            var user = await _uow.Users.GetByIdAsync(userId, cancellation) ?? throw new KeyNotFoundException("User not found.");

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<IEnumerable<AgentDto>> GetByRoleAsync(string roleName, CancellationToken cancellation = default)
        {
            return await _uow.UserRoles.GetAll()
                .Where(ur => ur.Role.RoleName == roleName)
                .Select(ur => new AgentDto
                {
                    UserId = ur.User.UserId,
                    FullName = ur.User.FullName,
                    Email = ur.User.Email,
                    IsActive = ur.User.IsActive,
                    ImageUrl = ur.User.ImageUrl,
                })
                .Distinct()
                .ToListAsync(cancellation);
        }
    }
}