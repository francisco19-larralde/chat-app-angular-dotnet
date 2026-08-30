using ChatApp.Application.DTOs.Friends;

namespace ChatApp.Application.Interfaces;

public interface IFriendService
{
    Task<List<UserSearchResultDto>> SearchUsersAsync(int currentUserId, string query);
    Task SendFriendRequestAsync(int requesterId, int addresseeId);
    Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId);
    Task AcceptFriendRequestAsync(int userId, int friendshipId);
    Task RejectFriendRequestAsync(int userId, int friendshipId);
    Task<List<FriendDto>> GetFriendsAsync(int userId);
    Task RemoveFriendAsync(int userId, int friendUserId);
}