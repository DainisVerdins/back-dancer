namespace Application.Entities.Common;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; } 
    public bool IsDescending { get; set; } = false;
}
