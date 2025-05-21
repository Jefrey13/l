using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class ContactLogService : IContactLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly INotificationService _notification;

        public ContactLogService(
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            INotificationService notification)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }

        public async Task<PagedResponse<ContactLogResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.ContactLogs.GetAll().OrderBy(cl => cl.Phone);
            var paged = await PagedList<ContactLog>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var dtos = paged.Select(cl => cl.Adapt<ContactLogResponseDto>()).ToList();
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

        public async Task UpdateAsync(UpdateContactLogRequestDto requestDto, CancellationToken cancellation = default)
        {
            var entity = await _uow.ContactLogs.GetByIdAsync(requestDto.Id, cancellation)
                ?? throw new KeyNotFoundException($"ContactLog {requestDto.Id} not found");

            requestDto.Adapt(entity);
            entity.Status = requestDto.Status;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.ContactLogs.Update(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellation = default)
        {
            var entity = await _uow.ContactLogs.GetByIdAsync(id, cancellation)
                ?? throw new KeyNotFoundException($"ContactLog {id} not found");
            _uow.ContactLogs.Remove(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<ContactLogResponseDto> GetOrCreateByPhoneAsync(
            string phone,
            string waId,
            string waName,
            string userId,
            CancellationToken cancellation = default)
                {
                    if (string.IsNullOrWhiteSpace(phone))
                        throw new ArgumentNullException(nameof(phone));

                    var existing = await _uow.ContactLogs
                        .GetAll()
                        .FirstOrDefaultAsync(c => c.Phone == phone, cancellation);

                    if (existing != null)
                        return existing.Adapt<ContactLogResponseDto>();

                    var contact = new ContactLog
                    {
                        Phone = phone,
                        WaId = waId,
                        WaName = waName,
                        WaUserId = userId,
                        Status = ContactStatus.New,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _uow.ContactLogs.AddAsync(contact, cancellation);
                    await _uow.SaveChangesAsync(cancellation);

                    return contact.Adapt<ContactLogResponseDto>();
        }
    }
}