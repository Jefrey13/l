using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface IWorkShiftService
    {
        Task<PagedResponse<WorkShiftResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default);
        Task<WorkShiftResponseDto> GetByIdAsync(int id, CancellationToken ct= default);
        Task<WorkShiftResponseDto> CreateAsync(WorkShiftRequestDto request, string jwtToken, CancellationToken ct = default);
        Task<WorkShiftResponseDto> UpdateAsync(int id, WorkShiftRequestDto request, string jwtToken, CancellationToken ct = default);

        //Change entity status from active to inactive and Vice versa
        Task<WorkShiftResponseDto> ToggleAsync(int id, string jwtToken, CancellationToken ct = default);
    }
}