using AutoMapper;
using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Template;
using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BrevoApi.Infrastructure.Services.Email;

public class TemplateService : ITemplateService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TemplateService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow; _mapper = mapper;
    }

    public async Task<TemplateDto?> GetByIdAsync(int id)
    {
        var t = await _uow.EmailTemplates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return t == null ? null : _mapper.Map<TemplateDto>(t);
    }

    public async Task<PagedResult<TemplateDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _uow.EmailTemplates.Query().Where(t => !t.IsDeleted);
        if (!string.IsNullOrEmpty(pagination.SearchTerm))
            query = query.Where(t => t.Name.Contains(pagination.SearchTerm));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize).ToListAsync();
        return new PagedResult<TemplateDto>
        {
            Items = _mapper.Map<IEnumerable<TemplateDto>>(items),
            TotalCount = total, PageNumber = pagination.PageNumber, PageSize = pagination.PageSize
        };
    }

    public async Task<TemplateDto> CreateAsync(CreateTemplateDto dto, int userId)
    {
        var template = _mapper.Map<Domain.Entities.EmailTemplate>(dto);
        template.CreatedBy = userId;
        await _uow.EmailTemplates.AddAsync(template);
        await _uow.SaveChangesAsync();
        return _mapper.Map<TemplateDto>(template);
    }

    public async Task<TemplateDto> UpdateAsync(int id, UpdateTemplateDto dto)
    {
        var template = await _uow.EmailTemplates.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Template bulunamadı: {id}");
        _mapper.Map(dto, template);
        template.UpdatedAt = DateTime.UtcNow;
        await _uow.UpdateAsync(template);
        await _uow.SaveChangesAsync();
        return _mapper.Map<TemplateDto>(template);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var t = await _uow.EmailTemplates.GetByIdAsync(id);
        if (t == null) return false;
        t.IsDeleted = true;
        await _uow.UpdateAsync(t);
        await _uow.SaveChangesAsync();
        return true;
    }

    public Task<bool> SyncWithBrevoAsync(int id) => Task.FromResult(true);

    public async Task<IEnumerable<TemplateDto>> GetActiveTemplatesAsync()
    {
        var templates = await _uow.EmailTemplates.FindAsync(
            t => t.Status == TemplateStatus.Active && !t.IsDeleted);
        return _mapper.Map<IEnumerable<TemplateDto>>(templates);
    }
}
