using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Profile.SoftDelete;

public interface ISoftDeleteService
{
    Task<SoftDeleteResult> DeleteAsync(Guid userId);
}

public class SoftDeleteService(IUserRepository userRepository, ILogger<SoftDeleteService> logger) : ISoftDeleteService
{
    public async Task<SoftDeleteResult> DeleteAsync(Guid userId)
    {
        var res = await userRepository.SoftDeleteAsync(userId);
        if (res != null)
        {
            logger.LogInformation("Пользователь {UserId} удалён", userId);
            return new()
            {
                Success = true,
                Message = "User deleted successfully"
            };
        }
        return new()
        {
            Success = false,
            Message = "User already deleted"
        };
    }
}
