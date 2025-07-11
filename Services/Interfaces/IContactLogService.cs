﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IContactLogService
    {
        Task<PagedResponse<ContactLogResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<IEnumerable<ContactLogResponseDto>> GetPendingApprovalAsync(CancellationToken cancellation = default);
        Task<ContactLogResponseDto> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<ContactLogResponseDto> CreateAsync(CreateContactLogRequestDto requestDto, CancellationToken cancellation = default);
        Task<ContactLogResponseDto> UpdateAsync(UpdateContactLogRequestDto requestDto, CancellationToken cancellation = default);
        Task<ContactLogResponseDto> GetByPhoneAsync(string phoneNumber, CancellationToken cancellation = default);
        Task<ContactLogResponseDto> ToggleAsync(int id, CancellationToken ct = default);

        Task<ContactLogResponseDto> GetOrCreateByPhoneAsync(string phone,
           string waId,
           string waName,
           string userId,
            CancellationToken cancellation = default);

        Task UpdateContactDetailsAsync(UpdateContactLogRequestDto requestDto, CancellationToken cancellation = default);

        Task VerifyAsync(int id, string jwtToken, CancellationToken ct = default);
    }
}