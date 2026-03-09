using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.User;

namespace BrevoApi.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int id);
    Task<PagedResult<UserDto>> GetAllAsync(PaginationParams pagination);
    Task<UserDto> UpdateAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignRoleAsync(int userId, string roleName);
    Task<bool> RemoveRoleAsync(int userId, string roleName);
    Task<IEnumerable<string>> GetRolesAsync(int userId);
    Task<bool> ToggleActiveStatusAsync(int userId);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
