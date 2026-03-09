using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.User;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Kendi profilimi getir</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _userService.GetCurrentUserAsync(GetCurrentUserId());
        return user == null ? NotFound() : OkResult(user);
    }

    /// <summary>Kendi profilimi güncelle</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
        => OkResult(await _userService.UpdateAsync(GetCurrentUserId(), dto));

    /// <summary>Tüm kullanıcıları getir (Admin)</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        => OkResult(await _userService.GetAllAsync(pagination));

    /// <summary>Kullanıcıyı ID ile getir (Admin)</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user == null ? NotFound() : OkResult(user);
    }

    /// <summary>Kullanıcı güncelle (Admin)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        => OkResult(await _userService.UpdateAsync(id, dto));

    /// <summary>Kullanıcı sil (Admin)</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    /// <summary>Aktif/Pasif toggle (Admin)</summary>
    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _userService.ToggleActiveStatusAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    /// <summary>Role ata (Admin)</summary>
    [HttpPost("{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleDto dto)
    {
        var result = await _userService.AssignRoleAsync(id, dto.RoleName);
        return result
            ? Ok(new { Success = true, Message = $"'{dto.RoleName}' rolü atandı." })
            : BadRequest(new { Message = "Role atama başarısız." });
    }

    /// <summary>Role kaldır (Admin)</summary>
    [HttpDelete("{id}/roles/{roleName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(int id, string roleName)
    {
        var result = await _userService.RemoveRoleAsync(id, roleName);
        return result
            ? Ok(new { Success = true })
            : BadRequest(new { Message = "Role kaldırma başarısız." });
    }

    /// <summary>Kullanıcı rollerini getir (Admin)</summary>
    [HttpGet("{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRoles(int id)
        => OkResult(await _userService.GetRolesAsync(id));
}
