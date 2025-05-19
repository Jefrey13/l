using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Utils;

namespace CustomerService.API.Services.Interfaces
{
    public interface ITagService
    {
        Task<PagedResponse<TagDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default);
        Task<TagDto> GetByIdAsync(int id, CancellationToken cancellation = default);
        Task<TagDto> CreateAsync(TagDto request, CancellationToken cancellation = default);
        Task UpdateAsync(TagDto request, CancellationToken cancellation = default);
        Task DeleteAsync(int id, CancellationToken cancellation = default);
    }
}