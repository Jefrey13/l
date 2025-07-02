using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface ISystemParamService
    {
        Task<SystemParamResponseDto> GetByIdAsync(int id);
        Task<SystemParamResponseDto> GetByNameAsync(string name);
        Task<IEnumerable<SystemParamResponseDto>> GetAllAsync();
        Task<PagedResponse<SystemParamResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default);
        Task<SystemParamResponseDto> CreateAsync(SystemParamRequestDto systemParam);
        Task<SystemParamResponseDto> UpdateAsync(SystemParamRequestDto systemParam);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<SystemParamResponseDto> ToggleAsync(int id, string jwtToken, CancellationToken ct = default);
    }
}