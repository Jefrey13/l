using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class TagService : ITagService
    {
        private readonly ITagRepository _tags;
        private readonly IUnitOfWork _uow;

        public TagService(ITagRepository tags, IUnitOfWork uow)
        {
            _tags = tags ?? throw new ArgumentNullException(nameof(tags));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PagedResponse<TagDto>> GetAllAsync(
            PaginationParams @params,
            CancellationToken cancellation = default)
        {
            var query = _uow.Tags.GetAll().OrderBy(t => t.Name);
            var paged = await PagedList<Tag>.CreateAsync(query, @params.PageNumber, @params.PageSize, cancellation);
            var dtos = paged.Select(t => t.Adapt<TagDto>()).ToList();
            return new PagedResponse<TagDto>(dtos, paged.MetaData);
        }

        public async Task<TagDto> GetByIdAsync(int id, CancellationToken cancellation = default)
        {
            var entity = await _uow.Tags.GetByIdAsync(id, cancellation)
                         ?? throw new KeyNotFoundException($"Tag {id} not found.");
            return entity.Adapt<TagDto>();
        }

        public async Task<TagDto> CreateAsync(TagDto request, CancellationToken cancellation = default)
        {
            if (await _tags.ExistsAsync(t => t.Name == request.Name, cancellation))
                throw new ArgumentException("Tag name already exists.");

            var entity = request.Adapt<Tag>();
            await _uow.Tags.AddAsync(entity, cancellation);
            await _uow.SaveChangesAsync(cancellation);
            return entity.Adapt<TagDto>();
        }

        public async Task UpdateAsync(TagDto request, CancellationToken cancellation = default)
        {
            var entity = await _uow.Tags.GetByIdAsync(request.TagId, cancellation)
                         ?? throw new KeyNotFoundException($"Tag {request.TagId} not found.");

            entity.Name = request.Name;
            _uow.Tags.Update(entity);
            await _uow.SaveChangesAsync(cancellation);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellation = default)
        {
            var entity = await _uow.Tags.GetByIdAsync(id, cancellation)
                         ?? throw new KeyNotFoundException($"Tag {id} not found.");

            _uow.Tags.Remove(entity);
            await _uow.SaveChangesAsync(cancellation);
        }
    }
}