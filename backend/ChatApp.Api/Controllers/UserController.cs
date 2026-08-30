using ChatApp.Application.DTOs.Users;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return int.Parse(idClaim!);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMyProfile()
    {
        var profile = await _userService.GetProfileAsync(GetCurrentUserId());
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMyProfile(UpdateProfileDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _userService.UpdateProfileAsync(GetCurrentUserId(), request);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("me/profile-picture")]
    public async Task<ActionResult<object>> UploadProfilePicture(IFormFile file)
    {
        if (!IsValidImage(file, out var error))
            return BadRequest(new { message = error });

        using var stream = file.OpenReadStream();
        var url = await _userService.UpdateProfilePictureAsync(GetCurrentUserId(), stream, file.FileName);

        return Ok(new { url });
    }

    [HttpPost("me/cover-picture")]
    public async Task<ActionResult<object>> UploadCoverPicture(IFormFile file)
    {
        if (!IsValidImage(file, out var error))
            return BadRequest(new { message = error });

        using var stream = file.OpenReadStream();
        var url = await _userService.UpdateCoverPictureAsync(GetCurrentUserId(), stream, file.FileName);

        return Ok(new { url });
    }


    private static bool IsValidImage(IFormFile? file, out string error)
    {
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "Debés seleccionar un archivo.";
            return false;
        }

        const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxSizeBytes)
        {
            error = "El archivo no puede superar los 5 MB.";
            return false;
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            error = "Solo se permiten imágenes JPG, PNG o WEBP.";
            return false;
        }

        return true;
    }
}