namespace ChatApp.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}