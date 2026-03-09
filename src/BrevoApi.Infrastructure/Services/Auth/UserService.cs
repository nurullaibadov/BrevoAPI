using AutoMapper;
using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.User;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrevoApi.Infrastructure.Services.Auth;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public UserService(UserManager<AppUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.IsDeleted) return null;
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = await _userManager.GetRolesAsync(user);
        return dto;
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _userManager.Users.Where(u => !u.IsDeleted);
        if (!string.IsNullOrEmpty(pagination.SearchTerm))
            query = query.Where(u => u.Email!.Contains(pagination.SearchTerm) ||
                u.FirstName.Contains(pagination.SearchTerm) ||
                u.LastName.Contains(pagination.SearchTerm));

        var total = await query.CountAsync();
        var users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize).ToListAsync();

        var dtos = new List<UserDto>();
        foreach (var user in users)
        {
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = await _userManager.GetRolesAsync(user);
            dtos.Add(dto);
        }

        return new PagedResult<UserDto>
        {
            Items = dtos, TotalCount = total,
            PageNumber = pagination.PageNumber, PageSize = pagination.PageSize
        };
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException($"Kullanıcı bulunamadı: {id}");
        _mapper.Map(dto, user);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var result = _mapper.Map<UserDto>(user);
        result.Roles = await _userManager.GetRolesAsync(user);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;
        user.IsDeleted = true;
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> AssignRoleAsync(int userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(int userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetRolesAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return Enumerable.Empty<string>();
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> ToggleActiveStatusAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId) => await GetByIdAsync(userId);
}
