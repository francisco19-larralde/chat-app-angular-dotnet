using ChatApp.Application.DTOs.Friends;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Services;

public class FriendService : IFriendService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;

    public FriendService(IFriendshipRepository friendshipRepository, IUserRepository userRepository)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
    }

    public async Task<List<UserSearchResultDto>> SearchUsersAsync(int currentUserId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<UserSearchResultDto>();

        var users = await _userRepository.SearchByUsernameAsync(query, currentUserId);
        var results = new List<UserSearchResultDto>();

        foreach (var user in users)
        {
            var friendship = await _friendshipRepository.GetByUsersAsync(currentUserId, user.Id);

            results.Add(new UserSearchResultDto
            {
                Id = user.Id,
                Username = user.Username,
                ProfilePictureUrl = user.ProfilePictureUrl,
                FriendshipStatus = friendship?.Status.ToString() ?? "None"
            });
        }

        return results;
    }

    public async Task SendFriendRequestAsync(int requesterId, int addresseeId)
    {
        if (requesterId == addresseeId)
            throw new InvalidOperationException("No podés agregarte a vos mismo como amigo.");

        var existing = await _friendshipRepository.GetByUsersAsync(requesterId, addresseeId);
        if (existing is not null)
            throw new InvalidOperationException("Ya existe una relación de amistad con este usuario.");

        var friendship = new Friendship
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendshipStatus.Pending
        };

        await _friendshipRepository.AddAsync(friendship);
        await _friendshipRepository.SaveChangesAsync();
    }

    public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(int userId)
    {
        var pending = await _friendshipRepository.GetPendingReceivedAsync(userId);

        return pending.Select(f => new FriendRequestDto
        {
            FriendshipId = f.Id,
            RequesterId = f.RequesterId,
            RequesterUsername = f.Requester.Username,
            RequesterProfilePictureUrl = f.Requester.ProfilePictureUrl,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task AcceptFriendRequestAsync(int userId, int friendshipId)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(friendshipId)
            ?? throw new KeyNotFoundException("Solicitud no encontrada.");

        // Solo quien RECIBIÓ la solicitud puede aceptarla, no quien la mandó
        if (friendship.AddresseeId != userId)
            throw new UnauthorizedAccessException("No podés aceptar esta solicitud.");

        friendship.Status = FriendshipStatus.Accepted;
        await _friendshipRepository.SaveChangesAsync();
    }

    public async Task RejectFriendRequestAsync(int userId, int friendshipId)
    {
        var friendship = await _friendshipRepository.GetByIdAsync(friendshipId)
            ?? throw new KeyNotFoundException("Solicitud no encontrada.");

        if (friendship.AddresseeId != userId)
            throw new UnauthorizedAccessException("No podés rechazar esta solicitud.");

        _friendshipRepository.Remove(friendship);
        await _friendshipRepository.SaveChangesAsync();
    }

    public async Task<List<FriendDto>> GetFriendsAsync(int userId)
    {
        var friendships = await _friendshipRepository.GetAcceptedFriendshipsAsync(userId);

        return friendships.Select(f =>
        {

            var friend = f.RequesterId == userId ? f.Addressee : f.Requester;

            return new FriendDto
            {
                UserId = friend.Id,
                Username = friend.Username,
                ProfilePictureUrl = friend.ProfilePictureUrl,
                IsOnline = friend.IsOnline,
                LastSeen = friend.LastSeen
            };
        }).ToList();
    }

    public async Task RemoveFriendAsync(int userId, int friendUserId)
    {
        var friendship = await _friendshipRepository.GetByUsersAsync(userId, friendUserId)
            ?? throw new KeyNotFoundException("No son amigos.");

        _friendshipRepository.Remove(friendship);
        await _friendshipRepository.SaveChangesAsync();
    }
}