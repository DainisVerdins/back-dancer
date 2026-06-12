namespace Application.Dtos;

public class RoleDto
{
    public string Title { set; get; } = string.Empty;
    public string Description { set; get; } = string.Empty;
    public string Icon { set; get; } = string.Empty;
    public string Color { set; get; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
}
