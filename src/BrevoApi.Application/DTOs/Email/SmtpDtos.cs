namespace BrevoApi.Application.DTOs.Email;

public class SmtpSettingsDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public bool UseStartTls { get; set; } = true;
    public string DefaultSenderName { get; set; } = string.Empty;
    public string DefaultSenderEmail { get; set; } = string.Empty;
}

public class SmtpSendRequestDto
{
    public List<EmailRecipientDto> To { get; set; } = new();
    public List<EmailRecipientDto>? Cc { get; set; }
    public List<EmailRecipientDto>? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public List<EmailAttachmentDto>? Attachments { get; set; }
    public int Priority { get; set; } = 3; // 1=High, 3=Normal, 5=Low
}

public class SmtpSendResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? FailedRecipients { get; set; }
}

public class SmtpTestRequestDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
}
