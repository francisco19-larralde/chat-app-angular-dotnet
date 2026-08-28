using ChatApp.Application.DTOs.Auth;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Settings;
using ChatApp.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new InvalidOperationException("Ya existe una cuenta con ese mail");


        if (await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException("Ya existe una cuenta con ese nombre de usuario");


        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || user.PasswordHash is null)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValida)
            throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException("Token de Google inválido.");
        }

        var user = await _userRepository.GetByGoogleIdAsync(payload.Subject);

        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(payload.Email);

            if (user is not null)
            {
                user.GoogleId = payload.Subject;
            }
            else
            {
                user = new User
                {
                    Username = await GenerateUniqueUsernameAsync(payload.Name ?? payload.Email),
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    ProfilePictureUrl = payload.Picture
                };
                await _userRepository.AddAsync(user);
            }

            await _userRepository.SaveChangesAsync();
        }

        return BuildAuthResponse(user);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string baseName)
    {
        var candidate = baseName.Replace(" ", "").ToLower();
        var attempt = candidate;
        var counter = 1;

        while (await _userRepository.ExistsByUsernameAsync(attempt))
        {
            counter++;
            attempt = $"{candidate}{counter}";
        }

        return attempt;
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }
}