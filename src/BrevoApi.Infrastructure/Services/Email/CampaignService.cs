using AutoMapper;
using BrevoApi.Application.Common;
using BrevoApi.Application.DTOs.Campaign;
using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Application.Interfaces.Services;
using BrevoApi.Domain.Entities;
using BrevoApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BrevoApi.Infrastructure.Services.Email;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IBrevoEmailService _emailService;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(IUnitOfWork uow, IMapper mapper,
        IBrevoEmailService emailService, ILogger<CampaignService> logger)
    {
        _uow = uow; _mapper = mapper; _emailService = emailService; _logger = logger;
    }

    public async Task<CampaignDto?> GetByIdAsync(int id)
    {
        var c = await _uow.Campaigns.Query()
            .Include(x => x.Template).Include(x => x.EmailList)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return c == null ? null : _mapper.Map<CampaignDto>(c);
    }

    public async Task<PagedResult<CampaignDto>> GetAllAsync(PaginationParams pagination)
    {
        var query = _uow.Campaigns.Query()
            .Include(c => c.Template).Include(c => c.EmailList)
            .Where(c => !c.IsDeleted);
        if (!string.IsNullOrEmpty(pagination.SearchTerm))
            query = query.Where(c => c.Name.Contains(pagination.SearchTerm));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize).ToListAsync();
        return new PagedResult<CampaignDto>
        {
            Items = _mapper.Map<IEnumerable<CampaignDto>>(items),
            TotalCount = total, PageNumber = pagination.PageNumber, PageSize = pagination.PageSize
        };
    }

    public async Task<CampaignDto> CreateAsync(CreateCampaignDto dto, int userId)
    {
        var campaign = _mapper.Map<Campaign>(dto);
        campaign.CreatedBy = userId;
        await _uow.Campaigns.AddAsync(campaign);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CampaignDto>(campaign);
    }

    public async Task<CampaignDto> UpdateAsync(int id, UpdateCampaignDto dto)
    {
        var campaign = await _uow.Campaigns.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Kampanya bulunamadı: {id}");
        _mapper.Map(dto, campaign);
        campaign.UpdatedAt = DateTime.UtcNow;
        await _uow.UpdateAsync(campaign);
        await _uow.SaveChangesAsync();
        return _mapper.Map<CampaignDto>(campaign);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var c = await _uow.Campaigns.GetByIdAsync(id);
        if (c == null) return false;
        c.IsDeleted = true;
        await _uow.UpdateAsync(c);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendAsync(int id)
    {
        var campaign = await _uow.Campaigns.Query()
            .Include(c => c.EmailList)
                .ThenInclude(l => l!.ContactListMappings)
                    .ThenInclude(m => m.Contact)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (campaign == null || campaign.Status != CampaignStatus.Draft) return false;

        campaign.Status = CampaignStatus.Sending;
        await _uow.UpdateAsync(campaign);
        await _uow.SaveChangesAsync();

        var contacts = campaign.EmailList?.ContactListMappings
            .Select(m => m.Contact)
            .Where(c => c.Status == ContactStatus.Active)
            .ToList() ?? new List<Contact>();

        int sent = 0;
        foreach (var contact in contacts)
        {
            var result = await _emailService.SendTransactionalEmailAsync(
                new Application.DTOs.Email.SendEmailRequestDto
                {
                    To = new List<Application.DTOs.Email.EmailRecipientDto>
                    {
                        new()
                        {
                            Email = contact.Email,
                            Name = $"{contact.FirstName} {contact.LastName}".Trim()
                        }
                    },
                    Subject = campaign.Subject,
                    HtmlContent = campaign.HtmlContent ?? "",
                    SenderName = campaign.SenderName,
                    SenderEmail = campaign.SenderEmail
                });

            await _uow.EmailLogs.AddAsync(new EmailLog
            {
                ToEmail = contact.Email,
                Subject = campaign.Subject,
                CampaignId = campaign.Id,
                ContactId = contact.Id,
                Status = result.Success ? EmailLogStatus.Sent : EmailLogStatus.Failed,
                BrevoMessageId = result.MessageId,
                SentAt = result.Success ? DateTime.UtcNow : null,
                EmailType = EmailType.Campaign,
                ErrorMessage = result.Success ? null : result.Message
            });
            if (result.Success) sent++;
        }

        campaign.Status = CampaignStatus.Sent;
        campaign.SentAt = DateTime.UtcNow;
        campaign.TotalRecipients = contacts.Count;
        await _uow.UpdateAsync(campaign);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Kampanya {Id} gönderildi. {Sent}/{Total}", id, sent, contacts.Count);
        return true;
    }

    public async Task<bool> ScheduleAsync(int id, DateTime scheduledAt)
    {
        var c = await _uow.Campaigns.GetByIdAsync(id);
        if (c == null) return false;
        c.Status = CampaignStatus.Scheduled;
        c.ScheduledAt = scheduledAt;
        await _uow.UpdateAsync(c);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PauseAsync(int id)
    {
        var c = await _uow.Campaigns.GetByIdAsync(id);
        if (c == null) return false;
        c.Status = CampaignStatus.Paused;
        await _uow.UpdateAsync(c);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var c = await _uow.Campaigns.GetByIdAsync(id);
        if (c == null) return false;
        c.Status = CampaignStatus.Cancelled;
        await _uow.UpdateAsync(c);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<CampaignStatsDto> GetStatsAsync(int id)
    {
        var c = await _uow.Campaigns.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Kampanya bulunamadı: {id}");
        return new CampaignStatsDto
        {
            Id = c.Id, Name = c.Name,
            TotalRecipients = c.TotalRecipients,
            TotalOpened = c.TotalOpened,
            TotalClicked = c.TotalClicked,
            TotalUnsubscribed = c.TotalUnsubscribed,
            TotalBounced = c.TotalBounced
        };
    }

    public Task SyncStatsFromBrevoAsync(int id) => Task.CompletedTask;
}
