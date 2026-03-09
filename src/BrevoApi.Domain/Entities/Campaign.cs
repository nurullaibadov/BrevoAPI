using BrevoApi.Domain.Common;
using BrevoApi.Domain.Enums;

namespace BrevoApi.Domain.Entities;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public int? EmailListId { get; set; }
    public string? HtmlContent { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public int? BrevoCampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int TotalRecipients { get; set; } = 0;
    public int TotalOpened { get; set; } = 0;
    public int TotalClicked { get; set; } = 0;
    public int TotalUnsubscribed { get; set; } = 0;
    public int TotalBounced { get; set; } = 0;
    public EmailTemplate? Template { get; set; }
    public EmailList? EmailList { get; set; }
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
