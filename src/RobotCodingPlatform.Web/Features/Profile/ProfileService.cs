using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Profile;

public interface IProfileService
{
    Task<User?> GetUserByIdAsync(Guid userId);
}

public class ProfileService(IUserRepository userRepository) : IProfileService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await userRepository.FindAsync(userId);
    }
}
