namespace Domain.Constants;

public static class UserRoleName
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Technical = "Technical";

    public static string[] GetRoleNames()
    {
        return [Admin, User, Technical];
    }
}
