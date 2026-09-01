using ChatApp.Application.DTOs.Users;
using ChatApp.Application.Interfaces;

namespace ChatApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public UserService(IUserRepository userRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        return MapToDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");


        if (user.Username != request.Username && await _userRepository.ExistsByUsernameAsync(request.Username))
            throw new InvalidOperationException("Ese nombre de usuario ya está en uso.");

        user.Username = request.Username;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<string> UpdateProfilePictureAsync(int userId, Stream fileStream, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");


        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            _fileStorageService.DeleteFile(user.ProfilePictureUrl);

        var newUrl = await _fileStorageService.SaveFileAsync(fileStream, fileName, "profile-pictures");

        user.ProfilePictureUrl = newUrl;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return newUrl;
    }

    public async Task<PublicUserProfileDto> GetPublicProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        return new PublicUserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CoverPictureUrl = user.CoverPictureUrl,
            IsOnline = user.IsOnline,
            LastSeen = user.LastSeen
        };
    }

    public async Task<string> UpdateCoverPictureAsync(int userId, Stream fileStream, string fileName)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!string.IsNullOrEmpty(user.CoverPictureUrl))
            _fileStorageService.DeleteFile(user.CoverPictureUrl);

        var newUrl = await _fileStorageService.SaveFileAsync(fileStream, fileName, "covers");

        user.CoverPictureUrl = newUrl;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return newUrl;
    }

    private static UserProfileDto MapToDto(Domain.Entities.User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CoverPictureUrl = user.CoverPictureUrl,
            IsOnline = user.IsOnline,
            LastSeen = user.LastSeen
        };
    }
}