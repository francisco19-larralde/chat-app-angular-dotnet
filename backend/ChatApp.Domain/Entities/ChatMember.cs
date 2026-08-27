namespace ChatApp.Domain.Entities;

public enum ChatRole
{
    Member,
    Admin
}


public class ChatMember
{
    public int Id { get; set; }

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ChatRole Role { get; set; } = ChatRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}