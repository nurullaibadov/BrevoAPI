using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class ContactsController : BaseController
{
    private readonly IContactService _contactService;
    public ContactsController(IContactService contactService) => _contactService = contactService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        => OkResult(await _contactService.GetAllAsync(pagination));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _contactService.GetByIdAsync(id);
        return c == null ? NotFound() : OkResult(c);
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var c = await _contactService.GetByEmailAsync(email);
        return c == null ? NotFound() : OkResult(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        var c = await _contactService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactDto dto)
        => OkResult(await _contactService.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _contactService.DeleteAsync(id);
        return result ? Ok(new { Success = true }) : NotFound();
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import([FromBody] List<CreateContactDto> contacts)
    {
        await _contactService.ImportContactsAsync(contacts);
        return Ok(new { Success = true, Message = $"{contacts.Count} contact import edildi." });
    }

    [HttpPost("{email}/unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe(string email)
        => Ok(new { Success = await _contactService.UnsubscribeAsync(email) });

    [HttpPost("{id}/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sync(int id)
        => Ok(new { Success = await _contactService.SyncWithBrevoAsync(id) });

    [HttpPost("{contactId}/lists/{listId}")]
    public async Task<IActionResult> AddToList(int contactId, int listId)
        => Ok(new { Success = await _contactService.AddToListAsync(contactId, listId) });

    [HttpDelete("{contactId}/lists/{listId}")]
    public async Task<IActionResult> RemoveFromList(int contactId, int listId)
        => Ok(new { Success = await _contactService.RemoveFromListAsync(contactId, listId) });

    [HttpGet("list/{listId}")]
    public async Task<IActionResult> GetByList(int listId)
        => OkResult(await _contactService.GetByListIdAsync(listId));
}
