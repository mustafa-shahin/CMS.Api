namespace CMS.Domain.Constants;

/// <summary>
/// Defines permission and policy constants for authorization.
/// </summary>
public static class Permissions
{
    // Policy names
    public const string RequireAdmin = "RequireAdminRole";
    public const string RequireAdminOrDeveloper = "RequireAdminOrDeveloperRole";
    public const string CanAccessDashboard = "CanAccessDashboard";
    public const string CanAccessDesigner = "CanAccessDesigner";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManagePages = "CanManagePages";
    public const string CanManageFiles = "CanManageFiles";
    public const string CanManageConfiguration = "CanManageConfiguration";

    // Claim types
    public const string UserIdClaimType = "uid";
    public const string EmailClaimType = "email";
    public const string RoleClaimType = "role";
    public const string FullNameClaimType = "name";
}