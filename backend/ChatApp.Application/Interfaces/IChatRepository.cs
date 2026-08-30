using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IChatRepository
{
    Task<Chat?> GetPrivateChatBetweenAsync(int userAId, int userBId);

    Task<Chat?> GetByIdAsync(int chatId);
    Task<bool> IsUserMemberAsync(int chatId, int userId);
    Task<List<Chat>> GetUserChatsAsync(int userId);

    Task AddAsync(Chat chat);
    Task AddMessageAsync(Message message);
    Task<List<Message>> GetMessagesAsync(int chatId, int skip, int take);

    Task SaveChangesAsync();
}