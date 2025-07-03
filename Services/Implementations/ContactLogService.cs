using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Hubs;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerService.API.Services.Implementations
{
    public class ContactLogService : IContactLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly INotificationService _notification;
        private readonly ITokenService _tokenService;
        private readonly IHubContext<NotificationsHub> _hubNotification;

        public ContactLogService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification,
            IHubContext<NotificationsHub> hubNotification,
            ITokenService tokenService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _hubNotification = hubNotification ?? throw new ArgumentException(nameof(_hubNotification));
            _tokenService = tokenService;
        }

        public async Task<PagedResponse<ContactLogResponseDto>> GetAllAsync([FromQuery] PaginationParams @params, CancellationToken ct = default)
        {
            var query = _uow.ContactLogs.GetAll().OrderBy(c=> c.CreatedAt);   

            var paged = await PagedList<ContactLog>.CreateAsync(query, @params.PageNumber, @params.PageSize, ct);

            var dtos = paged.Select(sp => sp.Adapt<ContactLogResponseDto>());

            return new PagedResponse<ContactLogResponseDto>(dtos, paged.MetaData);
        }

        public async Task<IEnumerable<ContactLogResponseDto>> GetPendingApprovalAsync(CancellationToken cancellation = default)
        {
            var list = await _uow.ContactLogs.GetAll()
                .Where(cl => cl.Status == ContactStatus.New || cl.Status == ContactStatus.PendingApproval)
                .ToListAsync(cancellation);
            return list.Adapt<IEnumerable<ContactLogResponseDto>>();
        }

        public async Task<ContactLogResponseDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            var entity = await _uow.ContactLogs.GetByIdAsync(id, cancellation)
                ?? throw new KeyNotFoundException($"ContactLog {id} not found");
            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task<ContactLogResponseDto> GetByPhoneAsync(string phoneNumber, CancellationToken cancellation = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));

            var entity = await _uow.ContactLogs.GetAll()
                .FirstOrDefaultAsync(cl => cl.Phone == phoneNumber, cancellation)
                ?? throw new KeyNotFoundException($"ContactLog {phoneNumber} not found");
            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task<ContactLogResponseDto> CreateAsync(CreateContactLogRequestDto requestDto, CancellationToken cancellation = default)
        {
            if (await _uow.ContactLogs.ExistsAsync(cl => cl.Phone == requestDto.Phone, cancellation))
                throw new ArgumentException($"Contact with phone {requestDto.Phone} already exists.");

            var entity = requestDto.Adapt<ContactLog>();
            entity.Status = ContactStatus.New;
            entity.CreatedAt = await _nicDatetime.GetNicDatetime();

            await _uow.ContactLogs.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);

            // notify all admins
            var adminIds = await _uow.Users.GetAll()
                .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName == "Admin"))
                .Select(u => u.UserId)
                .ToListAsync(cancellation);

            var payload = JsonSerializer.Serialize(new
            {
                ContactId = entity.Id,
                Phone = entity.Phone,
                WaName = entity.WaName
            });

            await _notification.CreateAsync(
                NotificationType.NewContact,
                payload,
                adminIds,
                cancellation);

            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task<ContactLogResponseDto> UpdateAsync(UpdateContactLogRequestDto requestDto, CancellationToken cancellation = default)
        {
            if(requestDto.Id <= 0)
                throw new ArgumentException("El id es obligatorio", nameof(requestDto.Id));


            var entity = await _uow.ContactLogs.GetByIdAsync(requestDto.Id, cancellation)
                ?? throw new KeyNotFoundException($"ContactLog {requestDto.Id} not found");

            requestDto.Adapt(entity);
            entity.Status = requestDto.Status;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.ContactLogs.Update(entity);
            await _uow.SaveChangesAsync(cancellation);

            return entity.Adapt<ContactLogResponseDto>();
        }
        public async Task VerifyAsync(int id, string jwtToken, CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentException("El id es obligatorio", nameof(id));

            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var entity = await _uow.ContactLogs.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"COntacto no encontrado.");

            entity.IsVerified = true;
            entity.VerifiedId = userId;
            entity.VerifiedAt = await _nicDatetime.GetNicDatetime();
            _uow.ContactLogs.Update(entity);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<ContactLogResponseDto> ToggleAsync(int id, CancellationToken ct = default)
        {
            var entity = await _uow.ContactLogs.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"ContactLog {id} not found");

            entity.IsActive = !entity.IsActive;

            _uow.ContactLogs.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<ContactLogResponseDto>();   
        }

        public async Task<ContactLogResponseDto> GetOrCreateByPhoneAsync(
        string phone,
        string waId,
        string waName,
        string userId,
        CancellationToken cancellation = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    throw new ArgumentNullException(nameof(phone));

                // 1) Buscar si ya existe
                var existing = await _uow.ContactLogs
                    .GetAll()
                    .FirstOrDefaultAsync(c => c.Phone == phone, cancellation);

                if (existing != null)
                {
                    var dirty = false;

                    // Solo sobreescribir si el webhook trae algo no‐vacío
                    if (!string.IsNullOrWhiteSpace(waName) && existing.WaName != waName)
                    {
                        existing.WaName = waName;
                        dirty = true;
                    }

                    if (!string.IsNullOrWhiteSpace(waId) && existing.WaId != waId)
                    {
                        existing.WaId = waId;
                        dirty = true;
                    }

                    // Si cambia también el teléfono aquí:
                    if (!string.IsNullOrWhiteSpace(phone) && existing.Phone != phone)
                    {
                        existing.Phone = phone;
                        dirty = true;
                    }

                    if (dirty)
                    {
                        existing.UpdatedAt = await _nicDatetime.GetNicDatetime();
                        _uow.ContactLogs.Update(existing);
                        await _uow.SaveChangesAsync(cancellation);
                    }

                    return existing.Adapt<ContactLogResponseDto>();
                }

                // Si no existe, crear
                var contact = new ContactLog
                {
                    Phone = phone,
                    WaId = waId,
                    WaName = waName,
                    WaUserId = userId,
                    Status = ContactStatus.New,
                    IdType = null,
                    CreatedAt = await _nicDatetime.GetNicDatetime()
                };

                await _uow.ContactLogs.AddAsync(contact, cancellation);
                await _uow.SaveChangesAsync(cancellation);


                var updatedContact = await _uow.ContactLogs.GetByPhone(phone);

                var dto = updatedContact.Adapt<ContactLogResponseDto>();

                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ContactLogResponseDto();
            }
        }

        public async Task UpdateContactDetailsAsync(UpdateContactLogRequestDto requestDto, CancellationToken ct = default)
        {
            try
            {
                var entity = await _uow.ContactLogs.GetByIdAsync(requestDto.Id, ct)
                         ?? throw new KeyNotFoundException($"ContactLog {requestDto.Id} not found");

                if (!string.IsNullOrWhiteSpace(requestDto.FullName))
                    entity.FullName = requestDto.FullName;

                if (!string.IsNullOrWhiteSpace(requestDto.IdType.ToString()))
                    entity.IdType = requestDto.IdType;

                if (!string.IsNullOrWhiteSpace(requestDto.IdCard))
                    entity.IdCard = requestDto.IdCard;

                if (!string.IsNullOrWhiteSpace(requestDto.Passport))
                    entity.Password = requestDto.Passport;

                if (!string.IsNullOrWhiteSpace(requestDto.ResidenceCard))
                    entity.ResidenceCard = requestDto.ResidenceCard;

                if (!string.IsNullOrWhiteSpace(requestDto.CompanyName))
                    entity.CompanyName = requestDto.CompanyName;

                if (requestDto.IsProvidingData != null)
                    entity.IsProvidingData = requestDto.IsProvidingData;

                entity.Status = requestDto.Status;
                entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

                await _uow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}