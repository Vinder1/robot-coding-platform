using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Profile.Update;

public interface IUpdateRoleService
{
    Task<bool> UpdateRoleAsync(Guid userId, string newRole);
}

public class UpdateRoleService(
    IUserRepository userRepository) : IUpdateRoleService
{
    public async Task<bool> UpdateRoleAsync(Guid userId, string newRole)
    {
        if (newRole is not (Roles.Admin or Roles.User))
        {
            return false;
        }
        var updatedUser = await userRepository.UpdateRoleAsync(userId, newRole);
        return updatedUser != null;
    }
}