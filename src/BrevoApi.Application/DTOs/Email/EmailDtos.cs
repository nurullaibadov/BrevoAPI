namespace BrevoApi.Application.DTOs.Email;

public class EmailRecipientDto
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class EmailAttachmentDto
{
    public string Content { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SendEmailRequestDto
{
    public List<EmailRecipientDto> To { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public List<EmailRecipientDto>? Cc { get; set; }
    public List<EmailRecipientDto>? Bcc { get; set; }
    public Dictionary<string, string>? Params { get; set; }
    public List<EmailAttachmentDto>? Attachments { get; set; }
}

public class SendTemplateEmailRequestDto
{
    public List<EmailRecipientDto> To { get; set; } = new();
    public int TemplateId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public Dictionary<string, string>? Params { get; set; }
}

public class BulkEmailRequestDto
{
    public List<SendEmailRequestDto> Emails { get; set; } = new();
}

public class SmtpEmailRequestDto
{
    public List<EmailRecipientDto> To { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
}

public class SendEmailResponseDto
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Message { get; set; }
}

public class EmailStatsDto
{
    public string MessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public string? Event { get; set; }
}
