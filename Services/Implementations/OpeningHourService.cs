using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
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

        public OpeningHourService(IUnitOfWork uow, 
            INicDatetime nicDateTime,
            ITokenService tokenService)
        {
            _uow = uow;
            _nicDateTime = nicDateTime;
            _tokenService = tokenService;
        }

        public async Task<OpeningHourResponseDto?> CreateAsync(OpeningHourRequestDto request, string jwtToken, CancellationToken ct = default)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromToken(jwtToken);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (request is null)
                    throw new ArgumentNullException(nameof(request), "El horario no puede ser null");

                var entity = request.Adapt<OpeningHour>();
                entity.CreatedAt = await _nicDateTime.GetNicDatetime();
                entity.CreatedById = userId;

                await _uow.OpeningHours.AddAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                return entity.Adapt<OpeningHourResponseDto>();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new OpeningHourResponseDto();
            }
        }

        public async Task<PagedResponse<OpeningHourResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default)
        {
            var query = _uow.OpeningHours.GetAll();
            var paged = await PagedList<OpeningHour>.CreateAsync(query, @params.PageNumber, @params.PageSize, ct);
            var dtos = paged.Select(op => op.Adapt<OpeningHourResponseDto>());

            return new PagedResponse<OpeningHourResponseDto>(dtos, paged.MetaData);
        }

        public async Task<OpeningHourResponseDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor que cero");

            var data = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"No se encontró horario con id {id}");

            return data.Adapt<OpeningHourResponseDto>();
        }

        public async Task<bool> IsHolidayAsync(CancellationToken ct = default)
        {
            return await _uow.OpeningHours.IsHolidayAsync(ct);
        }

        public async Task<bool> IsOutOfOpeningHour(CancellationToken ct = default)
        {
            return await _uow.OpeningHours.IsHolidayAsync(ct);
        }

        public async Task<OpeningHourResponseDto?> ToggleAsync(int id, string jwtToken, CancellationToken ct = default)
        {
            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id no es válido");

            var entity = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"No se encontró horario con id {id}");

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = await _nicDateTime.GetNicDatetime();
            entity.UpdatedById = userId;

            _uow.OpeningHours.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<OpeningHourResponseDto>();
        }

        public async Task<OpeningHourResponseDto?> UpdateAsync(int id, OpeningHourRequestDto request, string jwtToken, CancellationToken ct = default)
        {
            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El id no es válido");

            if (request is null)
                throw new ArgumentNullException(nameof(request), "El contenido no puede ser null");

            var entity = await _uow.OpeningHours.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"No se encontró horario con id {id}");

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.IsHoliday = request.IsHoliday;
            entity.UpdatedById = userId;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = await _nicDateTime.GetNicDatetime();

            _uow.OpeningHours.Update(entity, ct);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<OpeningHourResponseDto>();
        }
    }
}