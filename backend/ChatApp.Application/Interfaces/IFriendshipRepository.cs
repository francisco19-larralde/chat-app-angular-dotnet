using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByUsersAsync(int userAId, int userBId);
    Task<Friendship?> GetByIdAsync(int friendshipId);
    Task<List<Friendship>> GetPendingReceivedAsync(int userId);
    Task<List<Friendship>> GetAcceptedFriendshipsAsync(int userId);
    Task AddAsync(Friendship friendship);
    void Remove(Friendship friendship);
    Task SaveChangesAsync();
}