using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;

namespace BrevoApi.Application.Interfaces.Services;

public interface IContactService
{
    Task<ContactDto?> GetByIdAsync(int id);
    Task<ContactDto?> GetByEmailAsync(string email);
    Task<PagedResult<ContactDto>> GetAllAsync(PaginationParams pagination);
    Task<ContactDto> CreateAsync(CreateContactDto dto);
    Task<ContactDto> UpdateAsync(int id, UpdateContactDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ImportContactsAsync(IEnumerable<CreateContactDto> contacts);
    Task<bool> SyncWithBrevoAsync(int contactId);
    Task<bool> UnsubscribeAsync(string email);
    Task<bool> AddToListAsync(int contactId, int listId);
    Task<bool> RemoveFromListAsync(int contactId, int listId);
    Task<IEnumerable<ContactDto>> GetByListIdAsync(int listId);
}
