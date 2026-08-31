using ChatApp.Application.DTOs.Groups;

namespace ChatApp.Application.Interfaces;

public interface IGroupService
{
    Task<int> CreateGroupAsync(int creatorUserId, CreateGroupDto request);
    Task<GroupDetailsDto> GetGroupDetailsAsync(int userId, int chatId);
    Task AddMemberAsync(int requesterUserId, int chatId, int newMemberUserId);
    Task RemoveMemberAsync(int requesterUserId, int chatId, int memberUserIdToRemove);
    Task LeaveGroupAsync(int userId, int chatId);
    Task UpdateGroupAsync(int userId, int chatId, UpdateGroupDto request);
}