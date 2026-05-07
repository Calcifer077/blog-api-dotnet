using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BlogApi.Data;
using BlogApi.DTOs;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;

    public AuthService(AppDbContext db, IConfiguration config, IMapper mapper)
    {
        _db = db;
        _config = config;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
        // checks if user already exists
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("User already exists.");

        // map dto to user entity
        var user = _mapper.Map<User>(dto);
        // hash the password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // save user to database
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // return response with jwt token
        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = GenerateToken(user);
        return response;
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        // find user by email, if not found, throw error
        var user =
            await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        // matches hashed password and the password given to us now. done by BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        // if valid, generate token and return response
        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = GenerateToken(user);
        return response;
    }

    private string GenerateToken(User user)
    {
        // get secret key from config.
        var secret = _config["JwtSettings:Secret"];
        // create signing key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));

        // create claims, user info inside token.
        // these claims are stored inside the jwt, and can be used for authorization.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
        };

        // create token
        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        // convert token to string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
