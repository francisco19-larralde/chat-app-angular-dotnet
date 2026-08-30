using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly AppDbContext _context;

    public FriendshipRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Friendship?> GetByUsersAsync(int userAId, int userBId)
    {
        return await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.RequesterId == userAId && f.AddresseeId == userBId) ||
            (f.RequesterId == userBId && f.AddresseeId == userAId));
    }

    public async Task<Friendship?> GetByIdAsync(int friendshipId)
    {
        return await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .FirstOrDefaultAsync(f => f.Id == friendshipId);
    }

    public async Task<List<Friendship>> GetPendingReceivedAsync(int userId)
    {
        return await _context.Friendships
            .Include(f => f.Requester)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Friendship>> GetAcceptedFriendshipsAsync(int userId)
    {
        return await _context.Friendships
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.RequesterId == userId || f.AddresseeId == userId))
            .ToListAsync();
    }

    public async Task AddAsync(Friendship friendship)
    {
        await _context.Friendships.AddAsync(friendship);
    }

    public void Remove(Friendship friendship)
    {
        _context.Friendships.Remove(friendship);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}