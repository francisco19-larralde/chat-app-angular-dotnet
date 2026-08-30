using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return int.Parse(idClaim!);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var results = await _friendService.SearchUsersAsync(GetCurrentUserId(), query);
        return Ok(results);
    }

    [HttpPost("request/{addresseeId}")]
    public async Task<IActionResult> SendRequest(int addresseeId)
    {
        try
        {
            await _friendService.SendFriendRequestAsync(GetCurrentUserId(), addresseeId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _friendService.GetPendingRequestsAsync(GetCurrentUserId());
        return Ok(requests);
    }

    [HttpPost("requests/{friendshipId}/accept")]
    public async Task<IActionResult> AcceptRequest(int friendshipId)
    {
        try
        {
            await _friendService.AcceptFriendRequestAsync(GetCurrentUserId(), friendshipId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("requests/{friendshipId}/reject")]
    public async Task<IActionResult> RejectRequest(int friendshipId)
    {
        try
        {
            await _friendService.RejectFriendRequestAsync(GetCurrentUserId(), friendshipId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var friends = await _friendService.GetFriendsAsync(GetCurrentUserId());
        return Ok(friends);
    }

    [HttpDelete("{friendUserId}")]
    public async Task<IActionResult> RemoveFriend(int friendUserId)
    {
        try
        {
            await _friendService.RemoveFriendAsync(GetCurrentUserId(), friendUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}