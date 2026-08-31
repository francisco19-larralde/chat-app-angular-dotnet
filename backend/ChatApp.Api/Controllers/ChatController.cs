using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/chats")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return int.Parse(idClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChats()
    {
        var chats = await _chatService.GetUserChatsAsync(GetCurrentUserId());
        return Ok(chats);
    }

    [HttpPost("private/{otherUserId}")]
    public async Task<IActionResult> GetOrCreatePrivateChat(int otherUserId)
    {
        try
        {
            var chatId = await _chatService.GetOrCreatePrivateChatAsync(GetCurrentUserId(), otherUserId);
            return Ok(new { chatId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(int chatId, [FromQuery] int skip = 0, [FromQuery] int take = 30)
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(GetCurrentUserId(), chatId, skip, take);
            return Ok(messages);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(int chatId, [FromForm] SendMessageDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var message = await _chatService.SendMessageAsync(GetCurrentUserId(), chatId, request.Content, request.File);
            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}