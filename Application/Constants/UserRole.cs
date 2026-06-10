namespace Application.Constants;

public static class UserRole
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string ShelterWorker = "ShelterWorker";
    public const string User = "User";

    public static IEnumerable<string> GetRoleNames()
    {
        return [SuperAdmin, Admin, ShelterWorker, User];
    }
}
