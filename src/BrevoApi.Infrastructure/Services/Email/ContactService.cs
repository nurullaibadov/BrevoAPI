using AutoMapper;
using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;
using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Entities;
using BrevoApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace BrevoApi.Infrastructure.Services.Email;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IUnitOfWork uow, IMapper mapper, IHttpClientFactory httpClientFactory,
        IConfiguration configuration, ILogger<ContactService> logger)
    {
        _uow = uow; _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ContactDto?> GetByIdAsync(int id)
    {
        var contact = await _uow.Contacts.Query()
            .Include(c => c.ContactListMappings).ThenInclude(m => m.EmailList)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        return contact == null ? null : _mapper.Map<ContactDto>(contact);
    }

    public async Task<ContactDto?> GetByEmailAsync(string email)
    {
        var contact = await _uow.Contacts.Query()
            .Include(c => c.ContactListMappings).ThenInclude(m => m.EmailList)
            .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);
        return contact == null ? null : _mapper.Map<ContactDto>(contact);
    }

    public async Task<PagedResult<ContactDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _uow.Contacts.Query()
            .Include(c => c.ContactListMappings).ThenInclude(m => m.EmailList)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(pagination.SearchTerm))
            query = query.Where(c => c.Email.Contains(pagination.SearchTerm) ||
                (c.FirstName != null && c.FirstName.Contains(pagination.SearchTerm)) ||
                (c.LastName != null && c.LastName.Contains(pagination.SearchTerm)));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize).ToListAsync();

        return new PagedResult<ContactDto>
        {
            Items = _mapper.Map<IEnumerable<ContactDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto dto)
    {
        var contact = _mapper.Map<Contact>(dto);
        await _uow.Contacts.AddAsync(contact);
        await _uow.SaveChangesAsync();

        if (dto.ListIds != null)
            foreach (var listId in dto.ListIds)
                await _uow.ContactListMappings.AddAsync(new ContactListMapping
                {
                    ContactId = contact.Id,
                    EmailListId = listId
                });

        await _uow.SaveChangesAsync();
        _ = SyncWithBrevoAsync(contact.Id);
        return _mapper.Map<ContactDto>(contact);
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactDto dto)
    {
        var contact = await _uow.Contacts.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Contact {id} bulunamadı");
        _mapper.Map(dto, contact);
        contact.UpdatedAt = DateTime.UtcNow;
        await _uow.UpdateAsync(contact);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ContactDto>(contact);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contact = await _uow.Contacts.GetByIdAsync(id);
        if (contact == null) return false;
        contact.IsDeleted = true;
        await _uow.UpdateAsync(contact);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ImportContactsAsync(IEnumerable<CreateContactDto> contacts)
    {
        foreach (var dto in contacts)
        {
            var existing = await _uow.Contacts.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (existing == null) await CreateAsync(dto);
        }
        return true;
    }

    public async Task<bool> SyncWithBrevoAsync(int contactId)
    {
        try
        {
            var contact = await _uow.Contacts.GetByIdAsync(contactId);
            if (contact == null) return false;

            var client = _httpClientFactory.CreateClient("Brevo");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("api-key", _configuration["Brevo:ApiKey"]);

            var payload = new
            {
                email = contact.Email,
                attributes = new
                {
                    FIRSTNAME = contact.FirstName,
                    LASTNAME = contact.LastName,
                    SMS = contact.Phone
                }
            };
            var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/contacts", content);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                dynamic? result = JsonConvert.DeserializeObject(body);
                contact.BrevoContactId = result?.id?.ToString();
                await _uow.UpdateAsync(contact);
                await _uow.SaveChangesAsync();
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brevo contact sync hatası: {Id}", contactId);
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync(string email)
    {
        var contact = await _uow.Contacts.FirstOrDefaultAsync(c => c.Email == email);
        if (contact == null) return false;
        contact.Status = ContactStatus.Unsubscribed;
        await _uow.UpdateAsync(contact);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddToListAsync(int contactId, int listId)
    {
        var exists = await _uow.ContactListMappings.AnyAsync(
            m => m.ContactId == contactId && m.EmailListId == listId);
        if (exists) return true;
        await _uow.ContactListMappings.AddAsync(new ContactListMapping
        {
            ContactId = contactId, EmailListId = listId
        });
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromListAsync(int contactId, int listId)
    {
        var mapping = await _uow.ContactListMappings.FirstOrDefaultAsync(
            m => m.ContactId == contactId && m.EmailListId == listId);
        if (mapping == null) return false;
        await _uow.ContactListMappings.DeleteAsync(mapping);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ContactDto>> GetByListIdAsync(int listId)
    {
        var contacts = await _uow.Contacts.Query()
            .Include(c => c.ContactListMappings)
            .Where(c => c.ContactListMappings.Any(m => m.EmailListId == listId) && !c.IsDeleted)
            .ToListAsync();
        return _mapper.Map<IEnumerable<ContactDto>>(contacts);
    }
}
