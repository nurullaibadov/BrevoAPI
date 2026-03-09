using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;

namespace BrevoApi.Application.Interfaces.Services;

public interface IEmailListService
{
    Task<EmailListDto?> GetByIdAsync(int id);
    Task<PagedResult<EmailListDto>> GetAllAsync(PaginationParams pagination);
    Task<EmailListDto> CreateAsync(CreateEmailListDto dto, int userId);
    Task<EmailListDto> UpdateAsync(int id, UpdateEmailListDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> SyncWithBrevoAsync(int id);
    Task<IEnumerable<EmailListDto>> GetAllActiveAsync();
}
