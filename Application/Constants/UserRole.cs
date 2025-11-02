namespace Application.Constants;

public static class UserRole
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Guest = "Guest";
    public static IEnumerable<string> GetRoleNames()
    {
        return [Admin, User, Manager, Guest];
    }
}
