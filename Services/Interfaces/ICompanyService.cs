using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Dtos.RequestDtos;
namespace CustomerService.API.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyResponseDto>> GetAllAsync(CancellationToken cancellation = default);
        Task<CompanyResponseDto?> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<CompanyResponseDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellation = default);
        Task UpdateAsync(UpdateCompanyRequest request, CancellationToken cancellation = default);
        Task DeleteAsync(int id, CancellationToken cancellation = default);
    }
}
