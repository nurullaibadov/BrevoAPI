using BrevoApi.Application.DTOs.Email;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class EmailController : BaseController
{
    private readonly IBrevoEmailService _emailService;
    public EmailController(IBrevoEmailService emailService) => _emailService = emailService;

    /// <summary>Transactional email gönder</summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto request)
    {
        var result = await _emailService.SendTransactionalEmailAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Brevo template ile email gönder</summary>
    [HttpPost("send-template")]
    public async Task<IActionResult> SendTemplate([FromBody] SendTemplateEmailRequestDto request)
    {
        var result = await _emailService.SendTemplateEmailAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Toplu email gönder (Admin)</summary>
    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendBulk([FromBody] BulkEmailRequestDto request)
    {
        var result = await _emailService.SendBulkEmailAsync(request);
        return Ok(new { Success = result });
    }

    /// <summary>Email istatistikleri getir</summary>
    [HttpGet("stats/{messageId}")]
    public async Task<IActionResult> GetStats(string messageId)
        => OkResult(await _emailService.GetEmailStatsAsync(messageId));
}
