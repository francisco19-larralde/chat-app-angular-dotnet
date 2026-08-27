namespace ChatApp.Domain.Entities;

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Rejected
}

public class Friendship
{
    public int Id { get; set; }

    public int RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    public int AddresseeId { get; set; }
    public User Addressee { get; set; } = null!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}