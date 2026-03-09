using BrevoApi.Domain.Common;
using BrevoApi.Domain.Enums;

namespace BrevoApi.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public int? BrevoTemplateId { get; set; }
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
