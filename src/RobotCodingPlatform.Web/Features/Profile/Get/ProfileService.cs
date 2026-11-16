using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Profile.Get;

public interface IProfileService
{
    Task<User?> GetUserByIdAsync(Guid userId);
}

public class ProfileService(IUserRepository userRepository) : IProfileService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var user = await userRepository.FindActiveAsync(userId);
        if (user == null || user.DeletedAt != null)
        {
            return null;
        }
        return user;
    }
}


