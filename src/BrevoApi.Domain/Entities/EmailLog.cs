using BrevoApi.Domain.Common;
using BrevoApi.Domain.Enums;

namespace BrevoApi.Domain.Entities;

public class EmailLog : BaseEntity
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? BrevoMessageId { get; set; }
    public EmailLogStatus Status { get; set; } = EmailLogStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int? CampaignId { get; set; }
    public int? ContactId { get; set; }
    public int? UserId { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public EmailType EmailType { get; set; } = EmailType.Transactional;
    public Campaign? Campaign { get; set; }
    public Contact? Contact { get; set; }
    public AppUser? User { get; set; }
}
