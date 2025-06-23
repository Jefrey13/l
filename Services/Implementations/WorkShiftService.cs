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

        private void ValidateRequest(WorkShiftRequestDto request)
        {
            if (request.OpeningHourId <= 0)
                throw new ArgumentException("Invalid OpeningHourId", nameof(request.OpeningHourId));
            if (request.AssignedUserId <= 0)
                throw new ArgumentException("Invalid AssignedUserId", nameof(request.AssignedUserId));
            if (request.ValidFrom.HasValue && request.ValidTo.HasValue && request.ValidFrom > request.ValidTo)
                throw new ArgumentException("ValidFrom must be on or before ValidTo");
        }

        public async Task<WorkShiftResponseDto> CreateAsync(
            WorkShiftRequestDto request,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            var userId = await _tokenService.GetUserIdAsync(jwtToken);
            var now = await _nicDatetime.GetNicDatetime();

            var entity = request.Adapt<WorkShift_User>();
            entity.CreatedAt = now;
            entity.CreatedById = userId;
            entity.IsActive = request.IsActive;

            await _uow.WorkShifts.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<PagedResponse<WorkShiftResponseDto>> GetAllAsync(
            PaginationParams @params,
            CancellationToken ct = default)
        {
            var query = _uow.WorkShifts.GetAll();
            var paged = await PagedList<WorkShift_User>
                .CreateAsync(query, @params.PageNumber, @params.PageSize, ct);
            var dtos = paged.Select(ws => ws.Adapt<WorkShiftResponseDto>());
            return new PagedResponse<WorkShiftResponseDto>(dtos, paged.MetaData);
        }

        public async Task<WorkShiftResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"WorkShift {id} not found");

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<WorkShiftResponseDto> ToggleAsync(
            int id,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"WorkShift {id} not found");

            var userId = await _tokenService.GetUserIdAsync(jwtToken);
            entity.IsActive = !entity.IsActive;
            entity.UpdatedById = userId;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.WorkShifts.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public async Task<WorkShiftResponseDto> UpdateAsync(
            int id,
            WorkShiftRequestDto request,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            var entity = await _uow.WorkShifts.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"WorkShift {id} not found");

            var userId = await _tokenService.GetUserIdAsync(jwtToken);

            entity.OpeningHourId = request.OpeningHourId;
            entity.AssignedUserId = request.AssignedUserId;
            entity.ValidFrom = request.ValidFrom;
            entity.ValidTo = request.ValidTo;
            entity.IsActive = request.IsActive;
            entity.UpdatedById = userId;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.WorkShifts.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<WorkShiftResponseDto>();
        }

        public Task<int> GetActiveAssignmentsCountAsync(
            DateOnly date,
            CancellationToken ct = default) =>
            _uow.WorkShifts.GetActiveAssignmentsCountAsync(date, ct);

        public async Task<IEnumerable<WorkShiftResponseDto>> GetByDateAsync(
            DateOnly date,
            CancellationToken ct = default)
        {
            var list = await _uow.WorkShifts.GetByDateAsync(date, ct);
            return list.Select(ws => ws.Adapt<WorkShiftResponseDto>());
        }
    }
}