namespace CMS.Shared.Constants;

/// <summary>
/// Defines error codes used throughout the application for consistent error handling.
/// </summary>
public static class ErrorCodes
{
    // Authentication errors (1000-1099)
    public const string InvalidCredentials = "AUTH_1001";
    public const string AccountLocked = "AUTH_1002";
    public const string AccountInactive = "AUTH_1003";
    public const string InvalidToken = "AUTH_1004";
    public const string TokenExpired = "AUTH_1005";
    public const string RefreshTokenInvalid = "AUTH_1006";
    public const string RefreshTokenExpired = "AUTH_1007";
    public const string RefreshTokenRevoked = "AUTH_1008";
    public const string Unauthorized = "AUTH_1009";
    public const string Forbidden = "AUTH_1010";

    // Validation errors (2000-2099)
    public const string ValidationFailed = "VAL_2001";
    public const string InvalidEmail = "VAL_2002";
    public const string InvalidPassword = "VAL_2003";
    public const string PasswordTooWeak = "VAL_2004";
    public const string EmailAlreadyExists = "VAL_2005";
    public const string InvalidRole = "VAL_2006";

    // Resource errors (3000-3099)
    public const string NotFound = "RES_3001";
    public const string UserNotFound = "RES_3002";
    public const string PageNotFound = "RES_3003";
    public const string FileNotFound = "RES_3004";
    public const string FolderNotFound = "RES_3005";

    // Business logic errors (4000-4099)
    public const string CannotDeleteSelf = "BUS_4001";
    public const string CannotDeactivateSelf = "BUS_4002";
    public const string CannotChangeOwnRole = "BUS_4003";
    public const string LastAdminCannotBeDeleted = "BUS_4004";

    // Server errors (5000-5099)
    public const string InternalError = "SRV_5001";
    public const string DatabaseError = "SRV_5002";
    public const string ExternalServiceError = "SRV_5003";
}