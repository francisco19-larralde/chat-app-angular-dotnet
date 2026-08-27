namespace ChatApp.Domain.Entities;

public class Chat
{
    public int Id { get; set; }

    public bool IsGroup { get; set; }


    public string? Name { get; set; }
    public string? GroupPictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}