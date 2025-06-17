namespace Application.Exceptions;

public enum ErrorCode
{
    ArgumentIsEmpty,
    UserNotFound,
    InvalidCredentials,
    TokenExpired,
    RefreshTokenInvalid,
    OperationFailed,
    NotFound,
    RefreshTokenInCookieNotFound,
    InvalidPassword,
    UserBlocked,
    EmailLinkExpired,
    FileDoesNotExist
}
