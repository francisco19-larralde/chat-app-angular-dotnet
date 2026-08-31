using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IUserConnectionTracker _connectionTracker;
    private readonly IUserRepository _userRepository;
    private readonly IChatRepository _chatRepository;

    public ChatHub(
        IUserConnectionTracker connectionTracker,
        IUserRepository userRepository,
        IChatRepository chatRepository)
    {
        _connectionTracker = connectionTracker;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
    }

    private int GetCurrentUserId()
    {
        var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
        return int.Parse(idClaim!);
    }


    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();

        await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));

        var wasOffline = _connectionTracker.AddConnection(userId, Context.ConnectionId);

        if (wasOffline)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = true;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            await Clients.All.SendAsync("UserOnline", userId);
        }

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();

        var wentOffline = _connectionTracker.RemoveConnection(userId, Context.ConnectionId);

        if (wentOffline)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is not null)
            {
                user.IsOnline = false;
                user.LastSeen = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            await Clients.All.SendAsync("UserOffline", userId, DateTime.UtcNow);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(int chatId)
    {
        var userId = GetCurrentUserId();

        if (!await _chatRepository.IsUserMemberAsync(chatId, userId))
            throw new HubException("No tenés acceso a este chat.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GetChatGroupName(chatId));
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChatGroupName(chatId));
    }

    public static string GetChatGroupName(int chatId) => $"chat-{chatId}";
    public static string GetUserGroupName(int userId) => $"user-{userId}";
}