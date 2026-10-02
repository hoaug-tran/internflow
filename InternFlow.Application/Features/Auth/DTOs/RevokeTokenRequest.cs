using System.ComponentModel.DataAnnotations;

namespace InternFlow.Application.Features.Auth.DTOs;

public class RevokeTokenRequest
{
    [Required(ErrorMessage = "RefreshToken không được để trống")]
    public string RefreshToken { get; set; } = string.Empty;

    public string? Reason { get; set; }
}
