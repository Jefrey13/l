using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Models;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Services.Implementations
{
    public class ContactLogService: IContactLogService
    {
        private readonly IUnitOfWork _uow;

        public ContactLogService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ContactLogResponseDto> CreateAsync(CreateContactLogRequestDto requestDto, CancellationToken cancellation = default)
        {
            var exist = _uow.ContactLogs.GetByPhone(requestDto.Phone, cancellation) ??
                throw new NotImplementedException($"Ya existe un contacto resgistrado con el numero {requestDto.Phone}");

            var entity = requestDto.Adapt<ContactLog>();
            await _uow.ContactLogs.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);  
            
            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task DeleteAsync(int id, CancellationToken cancellation = default)
        {
            if (id <= 0)
                throw new ArgumentException($"No existe el registro de un contacto con id: {id}");

            var entity = await _uow.ContactLogs.GetByIdAsync(id, cancellation);
            entity.IsActive = !entity.IsActive;
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task<PagedResponse<ContactLogResponseDto>> GetAllAsync(PaginationParams @params, CancellationToken cancellation = default)
        {
            var query = _uow.ContactLogs.GetAll();
            var paged = await PagedList<ContactLog>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var contactLogs = paged.ToList();

            var dtos = paged
                .Select(cl => cl.Adapt<ContactLogResponseDto>())
                .ToList();

            return new PagedResponse<ContactLogResponseDto>(dtos, paged.MetaData);
        }

        public async Task<ContactLogResponseDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            var entity = await _uow.ContactLogs
                .GetByIdAsync(id, cancellation)
                ?? throw new KeyNotFoundException($"No se ha encontrado al usuario con el id {id}");

            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task<ContactLogResponseDto> GetByPhone(string phoneNumber, CancellationToken cancellation = default)
        {
            if (phoneNumber == null)
                throw new ArgumentNullException($"El numero telefonico es obligatorio");

            var entity = _uow.ContactLogs.GetByPhone(phoneNumber, cancellation);
            return entity.Adapt<ContactLogResponseDto>();
        }

        public async Task UpdateAsync(UpdateContactLogRequestDto requestDto, CancellationToken cancellation = default)
        {
           if(requestDto == null) throw new ArgumentNullException($"Obligatorio las propiedades del contacto", nameof(requestDto));

           var updatedEntity = requestDto.Adapt<ContactLog>();

            _uow.ContactLogs.Update(updatedEntity, cancellation);

            await _uow.SaveChangesAsync();
        }
    }
}