using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Mapster;

namespace CustomerService.API.Services.Implementations
{
    public class SystemParamService : ISystemParamService
    {
        private readonly ISystemParamRepository _systemParamRepository;
        private readonly ILogger<SystemParamService> _logger;
        private readonly IUnitOfWork _uow;
        private readonly INicDatetime _nicDatetime;
        public SystemParamService(ISystemParamRepository systemParamRepository,
            ILogger<SystemParamService> logger,
            IUnitOfWork uow,
            INicDatetime nicDatetime)
        {
            _systemParamRepository = systemParamRepository ?? throw new ArgumentNullException(nameof(systemParamRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            nicDatetime = nicDatetime ?? throw new ArgumentNullException(nameof(nicDatetime));
        }

        public async Task<SystemParamResponseDto> CreateAsync(SystemParamRequestDto systemParam)
        {
            //systemParam = systemParam ?? throw new ArgumentNullException(nameof(systemParam), "System parameter cannot be null.");
            if (systemParam == null)
            {
                _logger.LogError("System parameter request DTO is null.");
                throw new ArgumentNullException(nameof(systemParam), "System parameter cannot be null.");
                return new SystemParamResponseDto(); // This line is unreachable but added to satisfy the compiler
            }

            systemParam.Id = 0; // Ensure ID is set to 0 for new creation

            systemParam.CreatedAt = await _nicDatetime.GetNicDatetime();
            var createdEntity = systemParam.Adapt<SystemParam>();

            await _uow.SystemParamRepository.AddAsync(createdEntity);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Created system parameter with ID: {Id}", createdEntity.Id);

            var dto = createdEntity.Adapt<SystemParamResponseDto>();

            var responseDto = dto ?? throw new InvalidOperationException("Failed to map created entity to DTO.");
            return responseDto;
        }

        public async Task DeleteAsync(int id)
        {

            id = id <= 0 ? throw new ArgumentException("ID must be greater than zero.", nameof(id)) : id;
            //id = _uow.SystemParamRepository.ExistsAsync(id).Result ? id : throw new KeyNotFoundException($"System parameter with ID {id} not found.");

            id = _uow.SystemParamRepository.ExistsAsync(sp => sp.Id == id).Result 
                ? id 
                : throw new KeyNotFoundException($"System parameter with ID {id} not found.");

            var systemParam = _uow.SystemParamRepository.GetByIdAsync(id).Result;

            systemParam = systemParam ?? throw new KeyNotFoundException($"System parameter with ID {id} not found.");

            systemParam.IsActive = !systemParam.IsActive; // Toggle IsActive status
            systemParam.UpdateAt = await _nicDatetime.GetNicDatetime();

            _uow.SystemParamRepository.Update(systemParam);
            await _uow.SaveChangesAsync();

            var dto = systemParam.Adapt<SystemParamResponseDto>();
            if (dto == null)
            {
                _logger.LogWarning("System parameter with ID {Id} not found.", id);
                throw new KeyNotFoundException($"System parameter with ID {id} not found.");
            }
            _logger.LogInformation("Deleted system parameter with ID: {Id}", id);
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SystemParamResponseDto>> GetAllAsync()
        {
            var allParams = _uow.SystemParamRepository.GetAll();

            if (allParams == null || !allParams.Any())
            {
                _logger.LogWarning("No system parameters found.");
                return Task.FromResult<IEnumerable<SystemParamResponseDto>>(new List<SystemParamResponseDto>());
            }

            _logger.LogInformation("Retrieved {Count} system parameters.", allParams.Count());

            var dto = allParams
                .Select(sp => sp.Adapt<SystemParamResponseDto>())
                .ToList();
            return Task.FromResult<IEnumerable<SystemParamResponseDto>>(dto);
        }

        public async Task<SystemParamResponseDto> GetByIdAsync(int id)
        {
            if(id <= 0)
            {
                _logger.LogError("Invalid ID: {Id}", id);
                throw new ArgumentException("ID must be greater than zero.", nameof(id));
            }
            //if (!await _uow.SystemParamRepository.ExistsAsync(id))
            //{
            //    _logger.LogWarning("System parameter with ID {Id} does not exist.", id);
            //    throw new KeyNotFoundException($"System parameter with ID {id} not found.");
            //}
            _logger.LogInformation("Fetching system parameter with ID: {Id}", id);
            var systemParam = await _uow.SystemParamRepository.GetByIdAsync(id);

            return systemParam?.Adapt<SystemParamResponseDto>() 
                   ?? throw new KeyNotFoundException($"System parameter with ID {id} not found.");
        }

        public Task<SystemParamResponseDto> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogError("Invalid name: {Name}", name);
                throw new ArgumentException("Name cannot be null or empty.", nameof(name));
            }

            ////if ( _uow.SystemParamRepository.ExistsAsync(name))
            ////{
            ////    _logger.LogWarning("System parameter with name {Name} does not exist.", name);
            ////    throw new KeyNotFoundException($"System parameter with name {name} not found.");
            ////}

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Fetching system parameter with name: {Name}", name);
            }

            return _uow.SystemParamRepository.GetByNameAsync(name)
                .ContinueWith(task => task.Result?.Adapt<SystemParamResponseDto>()
                    ?? throw new KeyNotFoundException($"System parameter with name {name} not found."));
        }

        public Task<SystemParamResponseDto> UpdateAsync(SystemParamRequestDto systemParam)
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

           var updatedEntity = systemParam.Adapt<SystemParam>();
            //if (!_uow.SystemParamRepository.ExistsAsync(updatedEntity.Id).Result)
            //{
            //    _logger.LogWarning("System parameter with ID {Id} does not exist.", updatedEntity.Id);
            //    throw new KeyNotFoundException($"System parameter with ID {updatedEntity.Id} not found.");
            //}
            _uow.SystemParamRepository.Update(updatedEntity);
            _uow.SaveChangesAsync().Wait();
            _logger.LogInformation("Updated system parameter with ID: {Id}", updatedEntity.Id);
            return Task.FromResult(updatedEntity.Adapt<SystemParamResponseDto>());
        }
    }
}