using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

public class SignalRChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRChatNotifier(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessageAsync(int chatId, MessageDto message)
    {
        await _hubContext.Clients
            .Group(ChatHub.GetChatGroupName(chatId))
            .SendAsync("NewMessage", message);
    }

    public async Task NotifyChatListUpdateAsync(IEnumerable<int> memberUserIds, MessageDto message)
    {
        var groupNames = memberUserIds.Select(ChatHub.GetUserGroupName).ToList();

        await _hubContext.Clients
            .Groups(groupNames)
            .SendAsync("ChatUpdated", message);
    }
}