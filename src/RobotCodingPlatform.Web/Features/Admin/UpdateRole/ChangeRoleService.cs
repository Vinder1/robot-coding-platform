using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Admin.UpdateRole;

public interface IChangeRoleService
{
    Task<bool> ChangeRoleAsync(Guid userId, string newRole);
}

public class ChangeRoleService(
    IAdminUserRepository userRepository) : IChangeRoleService
{
    public async Task<bool> ChangeRoleAsync(Guid userId, string newRole)
    {
        if (newRole is not (Roles.Admin or Roles.User))
        {
            return false;
        }
        var updatedUser = await userRepository.ChangeRoleAsync(userId, newRole);
        return updatedUser != null;
    }
}