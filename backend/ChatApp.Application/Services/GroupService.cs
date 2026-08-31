using ChatApp.Application.DTOs.Groups;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Services;

public class GroupService : IGroupService
{
    private readonly IChatRepository _chatRepository;
    private readonly IUserRepository _userRepository;

    public GroupService(IChatRepository chatRepository, IUserRepository userRepository)
    {
        _chatRepository = chatRepository;
        _userRepository = userRepository;
    }

    public async Task<int> CreateGroupAsync(int creatorUserId, CreateGroupDto request)
    {
        var memberIds = request.MemberIds.Distinct().Where(id => id != creatorUserId).ToList();

        var chat = new Chat
        {
            IsGroup = true,
            Name = request.Name,
            Members = new List<ChatMember>
            {
                new() { UserId = creatorUserId, Role = ChatRole.Admin }
            }
        };

        foreach (var memberId in memberIds)
        {
            chat.Members.Add(new ChatMember { UserId = memberId, Role = ChatRole.Member });
        }

        await _chatRepository.AddAsync(chat);
        await _chatRepository.SaveChangesAsync();

        return chat.Id;
    }

    public async Task<GroupDetailsDto> GetGroupDetailsAsync(int userId, int chatId)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId)
            ?? throw new KeyNotFoundException("Grupo no encontrado.");

        if (!chat.IsGroup)
            throw new InvalidOperationException("Este chat no es un grupo.");

        var myMembership = chat.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new UnauthorizedAccessException("No sos miembro de este grupo.");

        return new GroupDetailsDto
        {
            ChatId = chat.Id,
            Name = chat.Name ?? string.Empty,
            GroupPictureUrl = chat.GroupPictureUrl,
            CurrentUserIsAdmin = myMembership.Role == ChatRole.Admin,
            Members = chat.Members.Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                ProfilePictureUrl = m.User.ProfilePictureUrl,
                IsOnline = m.User.IsOnline,
                Role = m.Role.ToString()
            }).ToList()
        };
    }

    public async Task AddMemberAsync(int requesterUserId, int chatId, int newMemberUserId)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId)
            ?? throw new KeyNotFoundException("Grupo no encontrado.");

        if (!chat.IsGroup)
            throw new InvalidOperationException("Este chat no es un grupo.");

        EnsureIsAdmin(chat, requesterUserId);

        if (chat.Members.Any(m => m.UserId == newMemberUserId))
            throw new InvalidOperationException("Ese usuario ya es miembro del grupo.");

        await _chatRepository.AddMemberAsync(new ChatMember
        {
            ChatId = chatId,
            UserId = newMemberUserId,
            Role = ChatRole.Member
        });

        await _chatRepository.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int requesterUserId, int chatId, int memberUserIdToRemove)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId)
            ?? throw new KeyNotFoundException("Grupo no encontrado.");

        if (!chat.IsGroup)
            throw new InvalidOperationException("Este chat no es un grupo.");

        EnsureIsAdmin(chat, requesterUserId);

        var memberToRemove = chat.Members.FirstOrDefault(m => m.UserId == memberUserIdToRemove)
            ?? throw new KeyNotFoundException("Ese usuario no es miembro del grupo.");


        if (memberToRemove.Role == ChatRole.Admin)
        {
            var adminCount = await _chatRepository.CountAdminsAsync(chatId);
            if (adminCount <= 1)
                throw new InvalidOperationException("No podés quitar al único administrador del grupo.");
        }

        _chatRepository.RemoveMember(memberToRemove);
        await _chatRepository.SaveChangesAsync();
    }

    public async Task LeaveGroupAsync(int userId, int chatId)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId)
            ?? throw new KeyNotFoundException("Grupo no encontrado.");

        var myMembership = chat.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new InvalidOperationException("No sos miembro de este grupo.");

        if (myMembership.Role == ChatRole.Admin)
        {
            var adminCount = await _chatRepository.CountAdminsAsync(chatId);
            if (adminCount <= 1 && chat.Members.Count > 1)
                throw new InvalidOperationException(
                    "Sos el único administrador. Asigná a otro miembro como administrador antes de salir.");
        }

        _chatRepository.RemoveMember(myMembership);
        await _chatRepository.SaveChangesAsync();
    }

    public async Task UpdateGroupAsync(int userId, int chatId, UpdateGroupDto request)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId)
            ?? throw new KeyNotFoundException("Grupo no encontrado.");

        if (!chat.IsGroup)
            throw new InvalidOperationException("Este chat no es un grupo.");

        EnsureIsAdmin(chat, userId);

        chat.Name = request.Name;
        await _chatRepository.SaveChangesAsync();
    }


    private static void EnsureIsAdmin(Chat chat, int userId)
    {
        var membership = chat.Members.FirstOrDefault(m => m.UserId == userId);

        if (membership is null)
            throw new UnauthorizedAccessException("No sos miembro de este grupo.");

        if (membership.Role != ChatRole.Admin)
            throw new UnauthorizedAccessException("Solo un administrador puede hacer esto.");
    }
}