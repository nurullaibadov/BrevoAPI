using BrevoApi.Domain.Enums;

namespace BrevoApi.Application.DTOs.Template;

public class TemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public int? BrevoTemplateId { get; set; }
    public TemplateStatus Status { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
}

public class UpdateTemplateDto
{
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? HtmlContent { get; set; }
    public string? TextContent { get; set; }
    public TemplateStatus? Status { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
}
