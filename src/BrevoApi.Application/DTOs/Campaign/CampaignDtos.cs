using BrevoApi.Domain.Enums;

namespace BrevoApi.Application.DTOs.Campaign;

public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public int? EmailListId { get; set; }
    public string? EmailListName { get; set; }
    public CampaignStatus Status { get; set; }
    public int? BrevoCampaignId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCampaignDto
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public int? TemplateId { get; set; }
    public int? EmailListId { get; set; }
    public string? HtmlContent { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class UpdateCampaignDto
{
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public int? TemplateId { get; set; }
    public int? EmailListId { get; set; }
    public string? HtmlContent { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class CampaignStatsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int TotalOpened { get; set; }
    public int TotalClicked { get; set; }
    public int TotalUnsubscribed { get; set; }
    public int TotalBounced { get; set; }
    public double OpenRate => TotalRecipients > 0 ? Math.Round((double)TotalOpened / TotalRecipients * 100, 2) : 0;
    public double ClickRate => TotalRecipients > 0 ? Math.Round((double)TotalClicked / TotalRecipients * 100, 2) : 0;
}

public class ScheduleCampaignDto
{
    public DateTime ScheduledAt { get; set; }
}
