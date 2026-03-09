using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Campaign;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class CampaignsController : BaseController
{
    private readonly ICampaignService _campaignService;
    public CampaignsController(ICampaignService campaignService) => _campaignService = campaignService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        => OkResult(await _campaignService.GetAllAsync(pagination));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _campaignService.GetByIdAsync(id);
        return c == null ? NotFound() : OkResult(c);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto)
    {
        var c = await _campaignService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignDto dto)
        => OkResult(await _campaignService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _campaignService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Send(int id)
    {
        var result = await _campaignService.SendAsync(id);
        return result
            ? Ok(new { Success = true, Message = "Kampanya gönderimi başladı." })
            : BadRequest(new { Message = "Kampanya gönderilemedi. Durumu kontrol edin." });
    }

    [HttpPost("{id}/schedule")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Schedule(int id, [FromBody] ScheduleCampaignDto dto)
    {
        var result = await _campaignService.ScheduleAsync(id, dto.ScheduledAt);
        return result
            ? Ok(new { Success = true, Message = $"Zamanlandı: {dto.ScheduledAt:u}" })
            : BadRequest(new { Message = "Zamanlama başarısız." });
    }

    [HttpPost("{id}/pause")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pause(int id)
        => Ok(new { Success = await _campaignService.PauseAsync(id) });

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(new { Success = await _campaignService.CancelAsync(id) });

    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetStats(int id)
        => OkResult(await _campaignService.GetStatsAsync(id));

    [HttpPost("{id}/sync-stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncStats(int id)
    {
        await _campaignService.SyncStatsFromBrevoAsync(id);
        return Ok(new { Success = true });
    }
}
