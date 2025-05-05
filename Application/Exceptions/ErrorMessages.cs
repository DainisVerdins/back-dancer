namespace Application.Exceptions;

public static class ErrorMessages
{
    private static readonly Dictionary<ErrorCode, string> Messages = new()
    {
        { ErrorCode.ArgumentIsEmpty, "The argument cannot be empty or null." },
        { ErrorCode.UserNotFound, "The specified user was not found." },
        { ErrorCode.InvalidCredentials, "The provided credentials are invalid." },
        { ErrorCode.TokenExpired, "The access token has expired." },
        { ErrorCode.RefreshTokenInvalid, "The refresh token is invalid or revoked." },
        { ErrorCode.OperationFailed, "The operation could not be completed." },
        { ErrorCode.NotFound, "Entity was not found in database" },
        { ErrorCode.RefreshTokenInCookieNotFound, "Refresh token cookie not found"},
        { ErrorCode.InvalidPassword, "Password is invalid"}
    };

    public static string GetMessage(ErrorCode errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message) ? message : "An unexpected error occurred.";
    }
}
