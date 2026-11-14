using FluentValidation;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Profile.Update;

public interface IUpdateProfileService
{
    Task<UpdateNameResponse> UpdateNameAsync(Guid userId, UpdateNameRequest request);
}

public class UpdateProfileService(
    IUserRepository userRepository,
    IValidator<UpdateNameRequest> validator) : IUpdateProfileService
{
    public async Task<UpdateNameResponse> UpdateNameAsync(Guid userId, UpdateNameRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return new UpdateNameResponse
            {
                Success = false,
                Errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)]
            };
        }

        var newUser = await userRepository.UpdateNameAsync(userId, request.Name);
        return new UpdateNameResponse
        {
            Success = newUser != null,
            Errors = newUser != null ? [] : ["User not found"]
        };
    }
}
