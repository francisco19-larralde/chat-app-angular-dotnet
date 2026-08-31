using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Chat?> GetPrivateChatBetweenAsync(int userAId, int userBId)
    {
        return await _context.Chats
            .Where(c => !c.IsGroup)
            .Where(c => c.Members.Any(m => m.UserId == userAId))
            .Where(c => c.Members.Any(m => m.UserId == userBId))
            .FirstOrDefaultAsync();
    }

    public async Task<Chat?> GetByIdAsync(int chatId)
    {
        return await _context.Chats
            .Include(c => c.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == chatId);
    }

    public async Task<bool> IsUserMemberAsync(int chatId, int userId)
    {
        return await _context.ChatMembers.AnyAsync(m => m.ChatId == chatId && m.UserId == userId);
    }

    public async Task<List<Chat>> GetUserChatsAsync(int userId)
    {
        return await _context.Chats
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task AddMemberAsync(ChatMember member)
    {
        await _context.ChatMembers.AddAsync(member);
    }

    public void RemoveMember(ChatMember member)
    {
        _context.ChatMembers.Remove(member);
    }

    public async Task<int> CountAdminsAsync(int chatId)
    {
        return await _context.ChatMembers
            .CountAsync(m => m.ChatId == chatId && m.Role == ChatRole.Admin);
    }

    public async Task AddAsync(Chat chat)
    {
        await _context.Chats.AddAsync(chat);
    }

    public async Task AddMessageAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }



    public async Task<List<Message>> GetMessagesAsync(int chatId, int skip, int take)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}