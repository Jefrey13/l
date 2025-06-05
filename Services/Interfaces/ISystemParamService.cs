using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;

namespace CustomerService.API.Services.Interfaces
{
    public interface ISystemParamService
    {
        Task<SystemParamResponseDto> GetByIdAsync(int id);
        Task<SystemParamResponseDto> GetByNameAsync(string name);
        Task<IEnumerable<SystemParamResponseDto>> GetAllAsync();
        Task<SystemParamResponseDto> CreateAsync(SystemParamRequestDto systemParam);
        Task<SystemParamResponseDto> UpdateAsync(SystemParamRequestDto systemParam);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}