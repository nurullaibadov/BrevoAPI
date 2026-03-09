using AutoMapper;
using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Contact;
using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace BrevoApi.Infrastructure.Services.Email;

public class EmailListService : IEmailListService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailListService> _logger;

    public EmailListService(IUnitOfWork uow, IMapper mapper, IHttpClientFactory httpClientFactory,
        IConfiguration configuration, ILogger<EmailListService> logger)
    {
        _uow = uow; _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<EmailListDto?> GetByIdAsync(int id)
    {
        var list = await _uow.EmailLists.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
        return list == null ? null : _mapper.Map<EmailListDto>(list);
    }

    public async Task<PagedResult<EmailListDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _uow.EmailLists.Query().Where(l => !l.IsDeleted);
        if (!string.IsNullOrEmpty(pagination.SearchTerm))
            query = query.Where(l => l.Name.Contains(pagination.SearchTerm));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize).ToListAsync();
        return new PagedResult<EmailListDto>
        {
            Items = _mapper.Map<IEnumerable<EmailListDto>>(items),
            TotalCount = total, PageNumber = pagination.PageNumber, PageSize = pagination.PageSize
        };
    }

    public async Task<EmailListDto> CreateAsync(CreateEmailListDto dto, int userId)
    {
        var list = _mapper.Map<EmailList>(dto);
        list.CreatedBy = userId;
        await _uow.EmailLists.AddAsync(list);
        await _uow.SaveChangesAsync();
        _ = SyncWithBrevoAsync(list.Id);
        return _mapper.Map<EmailListDto>(list);
    }

    public async Task<EmailListDto> UpdateAsync(int id, UpdateEmailListDto dto)
    {
        var list = await _uow.EmailLists.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Liste bulunamadı: {id}");
        _mapper.Map(dto, list);
        list.UpdatedAt = DateTime.UtcNow;
        await _uow.UpdateAsync(list);
        await _uow.SaveChangesAsync();
        return _mapper.Map<EmailListDto>(list);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var list = await _uow.EmailLists.GetByIdAsync(id);
        if (list == null) return false;
        list.IsDeleted = true;
        await _uow.UpdateAsync(list);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SyncWithBrevoAsync(int id)
    {
        try
        {
            var list = await _uow.EmailLists.GetByIdAsync(id);
            if (list == null) return false;
            var client = _httpClientFactory.CreateClient("Brevo");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("api-key", _configuration["Brevo:ApiKey"]);
            var payload = new { name = list.Name, folderId = 1 };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.brevo.com/v3/contacts/lists", content);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                dynamic? result = JsonConvert.DeserializeObject(body);
                list.BrevoListId = (int?)result?.id;
                await _uow.UpdateAsync(list);
                await _uow.SaveChangesAsync();
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { _logger.LogError(ex, "Liste sync hatası: {Id}", id); return false; }
    }

    public async Task<IEnumerable<EmailListDto>> GetAllActiveAsync()
    {
        var lists = await _uow.EmailLists.FindAsync(l => !l.IsDeleted);
        return _mapper.Map<IEnumerable<EmailListDto>>(lists);
    }
}
