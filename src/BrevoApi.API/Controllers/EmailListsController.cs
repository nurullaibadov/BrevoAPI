using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class EmailListsController : BaseController
{
    private readonly IEmailListService _listService;
    public EmailListsController(IEmailListService listService) => _listService = listService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        => OkResult(await _listService.GetAllAsync(pagination));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
        => OkResult(await _listService.GetAllActiveAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var list = await _listService.GetByIdAsync(id);
        return list == null ? NotFound() : OkResult(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmailListDto dto)
    {
        var list = await _listService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = list.Id }, list);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmailListDto dto)
        => OkResult(await _listService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _listService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("{id}/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sync(int id)
        => Ok(new { Success = await _listService.SyncWithBrevoAsync(id) });
}
