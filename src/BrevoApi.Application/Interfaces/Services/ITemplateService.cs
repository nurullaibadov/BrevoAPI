using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Template;

namespace BrevoApi.Application.Interfaces.Services;

public interface ITemplateService
{
    Task<TemplateDto?> GetByIdAsync(int id);
    Task<PagedResult<TemplateDto>> GetAllAsync(PaginationParams pagination);
    Task<TemplateDto> CreateAsync(CreateTemplateDto dto, int userId);
    Task<TemplateDto> UpdateAsync(int id, UpdateTemplateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> SyncWithBrevoAsync(int id);
    Task<IEnumerable<TemplateDto>> GetActiveTemplatesAsync();
}
