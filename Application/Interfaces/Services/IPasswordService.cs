namespace Application.Interfaces.Services;

public interface IPasswordService
{
    /// <summary>
    /// Empty list if password is valid, otherwise contains error messages
    /// </summary>
    /// <param name="password"></param>
    /// <returns>list of errors why validation failed</returns>
    Task<IList<string>> ValidatePasswordAsync(string password);

    /// <summary>
    /// Checks if provided password is same as hashedPassword value. This method could be used to check 
    /// if new password for user is not the same as it now have
    /// </summary>
    /// <param name="hashedPassword"></param>
    /// <param name="providedPassword"></param>
    /// <returns>Bool value if password are same or not</returns>
    //bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword);
}
