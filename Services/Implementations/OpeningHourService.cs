using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Utils.Enums;
using k8s.KubeConfigModels;
using Mapster;
using System.Security.Claims;

namespace CustomerService.API.Services.Implementations
{
    public class OpeningHourService : IOpeningHourService
    {
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDateTime;
        private readonly ITokenService _tokenService;

        public OpeningHourService(IUnitOfWork uow, INicDatetime nicDateTime, ITokenService tokenService)
        {
            _uow = uow;
            _nicDateTime = nicDateTime;
            _tokenService = tokenService;
        }

        private void ValidateRequest(OpeningHourRequestDto request)
        {
            if (request.StartTime >= request.EndTime)
                throw new ArgumentException("StartTime must be before EndTime");

            switch (request.Recurrence)
            {
                case RecurrenceType.Weekly:
                    // Para reglas semanales, se requieren días de la semana
                    if (request.DaysOfWeek == null || !request.DaysOfWeek.Any())
                        throw new ArgumentException("DaysOfWeek required for Weekly recurrence");
                    break;

                case RecurrenceType.AnnualHoliday:
                    // Para feriado anual, mes y día son obligatorios
                    if (request.HolidayDate == null)
                        throw new ArgumentException("HolidayDate required for AnnualHoliday recurrence");
                    break;

                case RecurrenceType.OneTimeHoliday:
                    // Para feriado único, fecha completa es obligatoria
                    if (request.SpecificDate == null)
                        throw new ArgumentException("SpecificDate required for OneTimeHoliday recurrence");
                    break;
            }

            if (request.EffectiveFrom.HasValue &&
                request.EffectiveTo.HasValue &&
                request.EffectiveFrom > request.EffectiveTo)
            {
                throw new ArgumentException("EffectiveFrom must be on or before EffectiveTo");
            }
        }

        public async Task<OpeningHourResponseDto?> CreateAsync(
            OpeningHourRequestDto request,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var now = await _nicDateTime.GetNicDatetime();
            var entity = request.Adapt<OpeningHour>();
            entity.CreatedAt = now;
            entity.CreatedById = userId;

            // TODO: validar solapamiento de horarios antes de guardar

            await _uow.OpeningHours.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<OpeningHourResponseDto>();
        }

        public async Task<PagedResponse<OpeningHourResponseDto>> GetAllAsync(
            PaginationParams @params,
            CancellationToken ct = default)
        {
            var query = _uow.OpeningHours.GetAll();
            var paged = await PagedList<OpeningHour>
                .CreateAsync(query, @params.PageNumber, @params.PageSize, ct);
            var dtos = paged.Select(oh => oh.Adapt<OpeningHourResponseDto>());
            return new PagedResponse<OpeningHourResponseDto>(dtos, paged.MetaData);
        }

        public async Task<OpeningHourResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var entity = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"OpeningHour {id} not found");

            return entity.Adapt<OpeningHourResponseDto>();
        }

        public Task<bool> IsHolidayAsync(DateOnly date, CancellationToken ct = default)
            => _uow.OpeningHours.IsHolidayAsync(date, ct);

        public Task<bool> IsOutOfOpeningHourAsync(DateTime instant, CancellationToken ct = default)
            => _uow.OpeningHours.IsOutOfOpeningHourAsync(instant, ct);

        public async Task<IEnumerable<OpeningHourResponseDto>> GetEffectiveScheduleAsync(
            DateOnly date,
            CancellationToken ct = default)
        {
            var list = await _uow.OpeningHours.GetEffectiveScheduleAsync(date, ct);
            return list.Select(oh => oh.Adapt<OpeningHourResponseDto>());
        }

        public async Task<OpeningHourResponseDto?> UpdateAsync(
            int id,
            OpeningHourRequestDto request,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            var entity = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"OpeningHour {id} not found");

            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            entity = request.Adapt(entity);
            entity.UpdatedAt = await _nicDateTime.GetNicDatetime();
            entity.UpdatedById = userId;

            // TODO: validar solapamiento tras cambios antes de guardar

            _uow.OpeningHours.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<OpeningHourResponseDto>();
        }

        public async Task<OpeningHourResponseDto?> ToggleAsync(
            int id,
            string jwtToken,
            CancellationToken ct = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var entity = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"OpeningHour {id} not found");

            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = await _nicDateTime.GetNicDatetime();
            entity.UpdatedById = userId;

            _uow.OpeningHours.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<OpeningHourResponseDto>();
        }
    }
}