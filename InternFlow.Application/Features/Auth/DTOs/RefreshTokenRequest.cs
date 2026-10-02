using System.ComponentModel.DataAnnotations;

namespace InternFlow.Application.Features.Auth.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "RefreshToken không được để trống")]
    public string RefreshToken { get; set; } = string.Empty;
}
