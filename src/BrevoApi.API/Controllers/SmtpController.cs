using BrevoApi.Application.DTOs.Email;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

/// <summary>SMTP email gönderme endpoints</summary>
[ApiVersion("1.0")]
[Authorize]
public class SmtpController : BaseController
{
    private readonly ISmtpEmailService _smtpService;

    public SmtpController(ISmtpEmailService smtpService)
        => _smtpService = smtpService;

    /// <summary>SMTP ile email gönder</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SmtpSendRequestDto request)
    {
        var result = await _smtpService.SendAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Template değişkenli SMTP email gönder ({{NAME}} formatı)</summary>
    [HttpPost("send-template")]
    public async Task<IActionResult> SendTemplate([FromBody] SmtpTemplateRequestDto request)
    {
        var result = await _smtpService.SendWithTemplateAsync(
            request.ToEmail,
            request.ToName ?? request.ToEmail,
            request.Subject,
            request.TemplateHtml,
            request.Variables,
            request.SenderName,
            request.SenderEmail);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>SMTP bağlantısını test et (Admin)</summary>
    [HttpGet("test-connection")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestConnection()
    {
        var success = await _smtpService.TestConnectionAsync();
        return Ok(new
        {
            Success = success,
            Message = success ? "SMTP bağlantısı başarılı." : "SMTP bağlantısı başarısız."
        });
    }

    /// <summary>Test emaili gönder (Admin)</summary>
    [HttpPost("send-test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendTest([FromBody] SmtpTestRequestDto request)
    {
        var result = await _smtpService.SendTestEmailAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// Extra DTO sadece bu controller için
public class SmtpTemplateRequestDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string TemplateHtml { get; set; } = string.Empty;
    public Dictionary<string, string>? Variables { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
}
