using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = "";
}

// DTOs/Auth/LoginDto.cs
public class LoginDto
{
    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

// DTOs/Auth/AuthResponseDto.cs
public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
}
