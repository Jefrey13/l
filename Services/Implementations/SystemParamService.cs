using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Mapster;
using Microsoft.Extensions.Logging;

namespace CustomerService.API.Services.Implementations
{
    public class SystemParamService : ISystemParamService
    {
        private readonly ISystemParamRepository _systemParamRepository;
        private readonly ILogger<SystemParamService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        private readonly ITokenService _tokenService;

        public SystemParamService(
            ISystemParamRepository systemParamRepository,
            ILogger<SystemParamService> logger,
            IUnitOfWork uow,
            INicDatetime nicDatetime,
            ITokenService tokenService)
        {
            _systemParamRepository = systemParamRepository
                ?? throw new ArgumentNullException(nameof(systemParamRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow
                ?? throw new ArgumentNullException(nameof(uow));
            _nicDatetime = nicDatetime
                ?? throw new ArgumentNullException(nameof(nicDatetime));
            _tokenService = tokenService;
        }

        public async Task<SystemParamResponseDto> CreateAsync(SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                _logger.LogError("System parameter request DTO is null.");
                throw new ArgumentNullException(nameof(systemParam), "System parameter cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(systemParam.Name))
            {
                _logger.LogError("System parameter Name is null or empty.");
                throw new ArgumentException("Name cannot be null or empty.", nameof(systemParam.Name));
            }

            if (string.IsNullOrWhiteSpace(systemParam.Value))
            {
                _logger.LogError("System parameter Value is null or empty.");
                throw new ArgumentException("Value cannot be null or empty.", nameof(systemParam.Value));
            }

            // Reset Id to 0 to force insert
            systemParam.Id = 0;

            // Map RequestDto → Entity
            var entity = systemParam.Adapt<SystemParam>();
            // Initialize creation timestamps
            entity.CreatedAt = await _nicDatetime.GetNicDatetime();
            entity.UpdatedAt = entity.CreatedAt;
            entity.IsActive = true;

            await _uow.SystemParamRepository.AddAsync(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Created SystemParam with ID: {Id}", entity.Id);

            // Map Entity → ResponseDto
            var responseDto = entity.Adapt<SystemParamResponseDto>();
            return responseDto;
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than zero.", nameof(id));

            bool exists = await _uow.SystemParamRepository.ExistsAsync(sp => sp.Id == id);
            if (!exists)
                throw new KeyNotFoundException($"System parameter with ID {id} not found.");

            var entity = await _uow.SystemParamRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"System parameter with ID {id} not found.");

            // Toggle IsActive (soft delete)
            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.SystemParamRepository.Update(entity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Toggled IsActive for SystemParam with ID: {Id}", id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0)
                return false;

            return await _uow.SystemParamRepository.ExistsAsync(sp => sp.Id == id);
        }

        public async Task<IEnumerable<SystemParamResponseDto>> GetAllAsync()
        {
            var allEntities = _uow.SystemParamRepository.GetAll().ToList();
            if (!allEntities.Any())
            {
                _logger.LogWarning("No system parameters found.");
                return Enumerable.Empty<SystemParamResponseDto>();
            }

            _logger.LogInformation("Retrieved {Count} system parameters.", allEntities.Count);

            var dtoList = allEntities
                .Select(sp => sp.Adapt<SystemParamResponseDto>())
                .ToList();

            return await Task.FromResult(dtoList);
        }
        public async Task<PagedResponse<SystemParamResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken ct = default)
        {
            try
            {
                var query = _uow.SystemParamRepository.GetAll();
                var paged = await PagedList<SystemParam>.CreateAsync(query, @params.PageNumber, @params.PageSize, ct);
                var dtos = paged.Select(sp => sp.Adapt<SystemParamResponseDto>());
                return new PagedResponse<SystemParamResponseDto>(dtos, paged.MetaData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recuperar SystemParams paginados");
                throw;
            }
        }
        public async Task<SystemParamResponseDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogError("Invalid ID: {Id}", id);
                throw new ArgumentException("ID must be greater than zero.", nameof(id));
            }

            _logger.LogInformation("Fetching SystemParam with ID: {Id}", id);
            var entity = await _uow.SystemParamRepository.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("SystemParam with ID {Id} not found.", id);
                throw new KeyNotFoundException($"System parameter with ID {id} not found.");
            }

            return entity.Adapt<SystemParamResponseDto>();
        }

        public async Task<SystemParamResponseDto> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogError("Invalid name: {Name}", name);
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            _logger.LogInformation("Fetching SystemParam with name: {Name}", name);
            var entity = await _uow.SystemParamRepository.GetByNameAsync(name);
            
            if (entity == null)
            {
                if (name == "ClientCriticalStateTime")
                {
                    // Devolvemos un DTO con valor por defecto
                    _logger.LogWarning("SystemParam '{Name}' not found. Returning default 60.", name);
                    return new SystemParamResponseDto
                    {
                        Name = name,
                        Value = "60"   // <- valor por defecto solo para ClientCriticalStateTime
                    };
                }

                _logger.LogWarning("SystemParam with name {Name} not found.", name);
                throw new KeyNotFoundException($"System parameter with name {name} not found.");
            }

            return entity.Adapt<SystemParamResponseDto>();
        }

        public async Task<SystemParamResponseDto> UpdateAsync(SystemParamRequestDto systemParam)
        {
            if (systemParam == null)
            {
                _logger.LogError("System parameter request DTO is null.");
                throw new ArgumentNullException(nameof(systemParam), "System parameter cannot be null.");
            }

            if (systemParam.Id <= 0)
            {
                _logger.LogError("Invalid ID: {Id}", systemParam.Id);
                throw new ArgumentException("ID must be greater than zero.", nameof(systemParam.Id));
            }

            bool exists = await _uow.SystemParamRepository.ExistsAsync(sp => sp.Id == systemParam.Id);
            if (!exists)
                throw new KeyNotFoundException($"System parameter with ID {systemParam.Id} not found.");

            // Retrieve existing entity (to preserve fields like CreatedAt, RowVersion)
            var existingEntity = await _uow.SystemParamRepository.GetByIdAsync(systemParam.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"System parameter with ID {systemParam.Id} not found.");

            // Map RequestDto → existing entity
            existingEntity.Name = systemParam.Name;
            existingEntity.Value = systemParam.Value;
            existingEntity.Description = systemParam.Description;
            existingEntity.Type = systemParam.Type;
            existingEntity.UpdatedAt = await _nicDatetime.GetNicDatetime();

            _uow.SystemParamRepository.Update(existingEntity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Updated SystemParam with ID: {Id}", existingEntity.Id);
            return existingEntity.Adapt<SystemParamResponseDto>();
        }

        public async Task<SystemParamResponseDto> ToggleAsync(
        int id,
        string jwtToken,
        CancellationToken ct = default
    )
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "El ID debe ser mayor que cero.");


            var entity = await _uow.SystemParamRepository.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException($"SystemParam {id} not found");

            var principal = _tokenService.GetPrincipalFromToken(jwtToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = await _nicDatetime.GetNicDatetime();
            entity.UpdateBy = userId;

            _uow.SystemParamRepository.Update(entity);
            await _uow.SaveChangesAsync(ct);

            return entity.Adapt<SystemParamResponseDto>();
        }
    }
}