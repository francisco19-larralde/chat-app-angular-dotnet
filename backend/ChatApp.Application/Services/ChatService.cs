using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace ChatApp.Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IChatNotifier _chatNotifier;
    private readonly IFileStorageService _fileStorageService;

    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg", "image/png", "image/webp", "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain", "application/zip"
    };
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public ChatService(
        IChatRepository chatRepository,
        IFriendshipRepository friendshipRepository,
        IChatNotifier chatNotifier,
        IFileStorageService fileStorageService)
    {
        _chatRepository = chatRepository;
        _friendshipRepository = friendshipRepository;
        _chatNotifier = chatNotifier;
        _fileStorageService = fileStorageService;
    }

    public async Task<List<ChatSummaryDto>> GetUserChatsAsync(int userId)
    {
        var chats = await _chatRepository.GetUserChatsAsync(userId);

        var summaries = new List<ChatSummaryDto>();

        foreach (var chat in chats)
        {
            var lastMessage = chat.Messages.FirstOrDefault();

            if (chat.IsGroup)
            {
                summaries.Add(new ChatSummaryDto
                {
                    ChatId = chat.Id,
                    IsGroup = true,
                    DisplayName = chat.Name ?? "Grupo sin nombre",
                    DisplayPictureUrl = chat.GroupPictureUrl,
                    LastMessageContent = lastMessage?.Content,
                    LastMessageAt = lastMessage?.SentAt
                });
            }
            else
            {

                var otherMember = chat.Members.First(m => m.UserId != userId).User;

                summaries.Add(new ChatSummaryDto
                {
                    ChatId = chat.Id,
                    IsGroup = false,
                    DisplayName = otherMember.Username,
                    DisplayPictureUrl = otherMember.ProfilePictureUrl,
                    OtherUserId = otherMember.Id,
                    LastMessageContent = lastMessage?.Content,
                    LastMessageAt = lastMessage?.SentAt,
                    IsOtherUserOnline = otherMember.IsOnline
                });
            }
        }

        return summaries.OrderByDescending(s => s.LastMessageAt).ToList();
    }

    public async Task<int> GetOrCreatePrivateChatAsync(int userId, int otherUserId)
    {
        if (userId == otherUserId)
            throw new InvalidOperationException("No podés iniciar un chat con vos mismo.");


        var friendship = await _friendshipRepository.GetByUsersAsync(userId, otherUserId);
        if (friendship is null || friendship.Status != FriendshipStatus.Accepted)
            throw new InvalidOperationException("Solo podés chatear con tus amigos.");

        var existingChat = await _chatRepository.GetPrivateChatBetweenAsync(userId, otherUserId);
        if (existingChat is not null)
            return existingChat.Id;

        var newChat = new Chat
        {
            IsGroup = false,
            Members = new List<ChatMember>
            {
                new() { UserId = userId },
                new() { UserId = otherUserId }
            }
        };

        await _chatRepository.AddAsync(newChat);
        await _chatRepository.SaveChangesAsync();

        return newChat.Id;
    }

    public async Task<List<MessageDto>> GetMessagesAsync(int userId, int chatId, int skip, int take)
    {
        if (!await _chatRepository.IsUserMemberAsync(chatId, userId))
            throw new UnauthorizedAccessException("No tenés acceso a este chat.");

        var messages = await _chatRepository.GetMessagesAsync(chatId, skip, take);

        return messages.Select(MapToDto).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(int userId, int chatId, string? content, IFormFile? file)
    {
        if (!await _chatRepository.IsUserMemberAsync(chatId, userId))
            throw new UnauthorizedAccessException("No tenés acceso a este chat.");

        var hasContent = !string.IsNullOrWhiteSpace(content);
        var hasFile = file is not null && file.Length > 0;

        if (!hasContent && !hasFile)
            throw new InvalidOperationException("El mensaje tiene que tener texto o un archivo adjunto.");

        if (hasFile)
            ValidateFile(file!);

        var message = new Message
        {
            ChatId = chatId,
            SenderId = userId,
            Content = hasContent ? content : null,
            SentAt = DateTime.UtcNow
        };

        if (hasFile)
        {
            using var stream = file!.OpenReadStream();
            var fileUrl = await _fileStorageService.SaveFileAsync(stream, file.FileName, "attachments");


            message.Attachments.Add(new Attachment
            {
                FileUrl = fileUrl,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length
            });
        }

        await _chatRepository.AddMessageAsync(message);
        await _chatRepository.SaveChangesAsync();

        var chat = await _chatRepository.GetByIdAsync(chatId);
        var sender = chat!.Members.First(m => m.UserId == userId).User;

        var messageDto = new MessageDto
        {
            Id = message.Id,
            ChatId = chatId,
            SenderId = userId,
            SenderUsername = sender.Username,
            SenderProfilePictureUrl = sender.ProfilePictureUrl,
            Content = message.Content,
            SentAt = message.SentAt,
            IsEdited = false,
            Attachments = message.Attachments.Select(MapAttachmentToDto).ToList()
        };

        await _chatNotifier.NotifyNewMessageAsync(chatId, messageDto);

        var memberIds = chat.Members.Select(m => m.UserId);
        await _chatNotifier.NotifyChatListUpdateAsync(memberIds, messageDto);

        return messageDto;
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("El archivo no puede superar los 20 MB.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Tipo de archivo no permitido.");
    }

    private static MessageDto MapToDto(Message m)
    {
        return new MessageDto
        {
            Id = m.Id,
            ChatId = m.ChatId,
            SenderId = m.SenderId,
            SenderUsername = m.Sender.Username,
            SenderProfilePictureUrl = m.Sender.ProfilePictureUrl,
            Content = m.Content,
            SentAt = m.SentAt,
            IsEdited = m.IsEdited,
            Attachments = m.Attachments.Select(MapAttachmentToDto).ToList()
        };
    }

    private static AttachmentDto MapAttachmentToDto(Attachment a)
    {
        return new AttachmentDto
        {
            Id = a.Id,
            FileUrl = a.FileUrl,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSizeBytes = a.FileSizeBytes
        };
    }
}