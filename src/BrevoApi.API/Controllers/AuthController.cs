using BrevoApi.Application.DTOs.Auth;
using BrevoApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrevoApi.API.Controllers;

[ApiVersion("1.0")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Yeni kullanıcı kaydı</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Email ve şifre ile giriş</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Access token yenile</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Çıkış yap (refresh token iptal et)</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(GetCurrentUserId());
        return Ok(new { Success = true, Message = "Çıkış yapıldı." });
    }

    /// <summary>Şifre sıfırlama emaili gönder</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { Success = true, Message = "Email gönderildi (hesap mevcutsa)." });
    }

    /// <summary>Token ile şifre sıfırla</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result
            ? Ok(new { Success = true, Message = "Şifre güncellendi." })
            : BadRequest(new { Success = false, Message = "Geçersiz veya süresi dolmuş token." });
    }

    /// <summary>Şifre değiştir (giriş yapmış kullanıcı)</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var result = await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
        return result
            ? Ok(new { Success = true, Message = "Şifre değiştirildi." })
            : BadRequest(new { Success = false, Message = "Mevcut şifre hatalı." });
    }

    /// <summary>Email onayla</summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto request)
    {
        var result = await _authService.ConfirmEmailAsync(request);
        return result
            ? Ok(new { Success = true, Message = "Email onaylandı." })
            : BadRequest(new { Success = false, Message = "Geçersiz token." });
    }

    /// <summary>Email onay mailini tekrar gönder</summary>
    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendConfirmation([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ResendConfirmationEmailAsync(request.Email);
        return Ok(new { Success = true, Message = "Onay emaili gönderildi." });
    }
}
