namespace RobotCodingPlatform.Web.Features.Admin.UpdateRole;

public class ChangeRoleRequest
{
    public string Role { get; init; } = null!; // "User" или "Admin"
}