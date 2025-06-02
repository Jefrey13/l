using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;
namespace CustomerService.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<UserResponseDto> CreateAsync(CreateUserRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellation = default);
        Task ActivationAsync(int id, CancellationToken cancellation = default);
        Task<IEnumerable<AgentDto>> GetByRoleAsync(string roleName, CancellationToken cancellation = default);

    }
}