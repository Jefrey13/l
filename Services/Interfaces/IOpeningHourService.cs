using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IOpeningHourService
    {
        Task<PagedResponse<OpeningHourResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default);
        Task<OpeningHourResponseDto> GetByIdAsync(int id, CancellationToken ct = default);

        Task<OpeningHourResponseDto?> CreateAsync(OpeningHourRequestDto request, string jwtToken, CancellationToken ct = default);

        Task<OpeningHourResponseDto?> UpdateAsync(int id, OpeningHourRequestDto request, string jwtToken, CancellationToken ct = default);
        Task<OpeningHourResponseDto?> ToggleAsync(int id, string jwtToken,CancellationToken ct = default);
    }
}
