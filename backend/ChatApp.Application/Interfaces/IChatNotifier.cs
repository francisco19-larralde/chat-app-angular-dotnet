using ChatApp.Application.DTOs.Chats;

namespace ChatApp.Application.Interfaces;

public interface IChatNotifier
{
    Task NotifyNewMessageAsync(int chatId, MessageDto message);
    Task NotifyChatListUpdateAsync(IEnumerable<int> memberUserIds, MessageDto message);
}
