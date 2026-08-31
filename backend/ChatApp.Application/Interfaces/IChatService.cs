using ChatApp.Application.DTOs.Chats;
using Microsoft.AspNetCore.Http;

namespace ChatApp.Application.Interfaces;

public interface IChatService
{
    Task<List<ChatSummaryDto>> GetUserChatsAsync(int userId);
    Task<int> GetOrCreatePrivateChatAsync(int userId, int otherUserId);
    Task<List<MessageDto>> GetMessagesAsync(int userId, int chatId, int skip, int take);

    Task<MessageDto> SendMessageAsync(int userId, int chatId, string? content, IFormFile? file);
}