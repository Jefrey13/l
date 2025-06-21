using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using k8s.Models;
using Mapster;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomerService.API.Services.Implementations
{
    public class WorkShiftService : IWorkShiftService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly INicDatetime _nicDatetime;
        public WorkShiftService(IUnitOfWork uow, ITokenService tokenService, INicDatetime nicDatetime)
        {
            _uow = uow;
            _tokenService = tokenService;
            _nicDatetime = nicDatetime;
        }
        public async Task<WorkShiftResponseDto> CreateAsync(WorkShiftRequestDto request, string jwtToken, CancellationToken ct = default)
        {
            if(request == null) throw new ArgumentNullException("Solicitud invalida. No puede ser null", nameof(request));

            var entity = request.Adapt<WorkShift_User>();

            await _uow.WorkShifts.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<PagedResponse<WorkShiftResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default)
        {
            var query = _uow.WorkShifts.GetAll();
            var paged = await PagedList<WorkShift_User>.CreateAsync(query, @params.PageNumber, @params.PageSize, ct);
            var dto = paged.Select(ws => ws.Adapt<WorkShiftResponseDto>());

            return new PagedResponse<WorkShiftResponseDto>(dto, paged.MetaData);
        }

        public async Task<WorkShiftResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if(id <= 0) {  throw new ArgumentOutOfRangeException("El id no puede ser null", nameof(id));}

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<WorkShiftResponseDto> ToggleAsync(int id, string jwtToken, CancellationToken ct = default)
        {
           var userId = await _tokenService.GetUserIdAsync(jwtToken);

            if (id <= 0) { throw new ArgumentException("El id no puede ser null", nameof(id)); }

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct);

            entity.IsActive = !entity.IsActive;
            entity.UpdatedById = userId;

             _uow.WorkShifts.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<WorkShiftResponseDto> UpdateAsync(int id, WorkShiftRequestDto request, string jwtToken, CancellationToken ct = default)
        {
            var userId = await _tokenService.GetUserIdAsync(jwtToken);

            if (id <= 0) { throw new ArgumentException("El id del turno a actualizar no puede ser null ", nameof(id)); }
            if(request == null) { throw new ArgumentNullException("Los datos ha actualizar no pueden ser null", nameof(request)); }

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct);

            entity.OpeningHourId = request.OpeningHourId;
            entity.AssingedUserId = request.AssingedUserId;
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();
            entity.UpdatedById = userId;

            return entity.Adapt<WorkShiftResponseDto>();
        }
    }
}