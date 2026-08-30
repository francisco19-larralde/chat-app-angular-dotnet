using ChatApp.Application.DTOs.Users;

namespace ChatApp.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto request);
    Task<string> UpdateProfilePictureAsync(int userId, Stream fileStream, string fileName);
    Task<string> UpdateCoverPictureAsync(int userId, Stream fileStream, string fileName);
}