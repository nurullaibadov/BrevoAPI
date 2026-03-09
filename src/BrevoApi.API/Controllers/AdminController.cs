using BrevoApi.Application.Common;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IUserService _userService;
    private readonly IContactService _contactService;
    private readonly ICampaignService _campaignService;

    public AdminController(IUserService userService, IContactService contactService, ICampaignService campaignService)
    {
        _userService = userService;
        _contactService = contactService;
        _campaignService = campaignService;
    }

    /// <summary>Dashboard istatistikleri</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var users = await _userService.GetAllAsync(new PaginationParams { PageSize = 1 });
        var contacts = await _contactService.GetAllAsync(new PaginationParams { PageSize = 1 });
        var campaigns = await _campaignService.GetAllAsync(new PaginationParams { PageSize = 1 });
        return Ok(new
        {
            TotalUsers = users.TotalCount,
            TotalContacts = contacts.TotalCount,
            TotalCampaigns = campaigns.TotalCount,
            GeneratedAt = DateTime.UtcNow
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination)
        => OkResult(await _userService.GetAllAsync(pagination));

    [HttpPatch("users/{id}/toggle")]
    public async Task<IActionResult> ToggleUser(int id)
    {
        var result = await _userService.ToggleActiveStatusAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (id == GetCurrentUserId())
            return BadRequest(new { Message = "Kendi hesabınızı silemezsiniz." });
        var result = await _userService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }
}
