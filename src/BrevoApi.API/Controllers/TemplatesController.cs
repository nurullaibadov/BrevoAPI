using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Template;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class TemplatesController : BaseController
{
    private readonly ITemplateService _templateService;
    public TemplatesController(ITemplateService templateService) => _templateService = templateService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        => OkResult(await _templateService.GetAllAsync(pagination));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => OkResult(await _templateService.GetActiveTemplatesAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await _templateService.GetByIdAsync(id);
        return t == null ? NotFound() : OkResult(t);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto dto)
    {
        var t = await _templateService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTemplateDto dto)
        => OkResult(await _templateService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _templateService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("{id}/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sync(int id)
        => Ok(new { Success = await _templateService.SyncWithBrevoAsync(id) });
}
