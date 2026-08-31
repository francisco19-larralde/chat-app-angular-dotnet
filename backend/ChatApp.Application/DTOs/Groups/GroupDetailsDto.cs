namespace ChatApp.Application.DTOs.Groups;

public class GroupDetailsDto
{
    public int ChatId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GroupPictureUrl { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();

    public bool CurrentUserIsAdmin { get; set; }
}