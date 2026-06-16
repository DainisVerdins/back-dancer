namespace Application.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ActiveRole { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> AvailableRoles { get; set; } = [];
}
