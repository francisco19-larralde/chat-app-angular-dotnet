using ChatApp.Application.DTOs.Groups;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        return int.Parse(idClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup(CreateGroupDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var chatId = await _groupService.CreateGroupAsync(GetCurrentUserId(), request);
        return Ok(new { chatId });
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetGroupDetails(int chatId)
    {
        try
        {
            var details = await _groupService.GetGroupDetailsAsync(GetCurrentUserId(), chatId);
            return Ok(details);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{chatId}/members/{newMemberUserId}")]
    public async Task<IActionResult> AddMember(int chatId, int newMemberUserId)
    {
        try
        {
            await _groupService.AddMemberAsync(GetCurrentUserId(), chatId, newMemberUserId);
            return Ok();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{chatId}/members/{memberUserId}")]
    public async Task<IActionResult> RemoveMember(int chatId, int memberUserId)
    {
        try
        {
            await _groupService.RemoveMemberAsync(GetCurrentUserId(), chatId, memberUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{chatId}/leave")]
    public async Task<IActionResult> LeaveGroup(int chatId)
    {
        try
        {
            await _groupService.LeaveGroupAsync(GetCurrentUserId(), chatId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{chatId}")]
    public async Task<IActionResult> UpdateGroup(int chatId, UpdateGroupDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _groupService.UpdateGroupAsync(GetCurrentUserId(), chatId, request);
            return Ok();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}