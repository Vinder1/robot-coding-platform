using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace RobotCodingPlatform.Web.Features.User.SignIn;

public class SignInService(
    ILogger<SignInService> logger,
    IPasswordHasher<User> passwordHasher,
    IValidator<SignInRequest> validator,
    IUserRepository userRepository)
{
    private readonly ILogger<SignInService> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher
        ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IUserRepository _userRepository = userRepository
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IValidator<SignInRequest> _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return new SignInResponse
            {
                Success = false,
                Errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)]
            };
        }

        if (await _userRepository.ExistsWithEmail(request.Email))
        {
            return new SignInResponse
            {
                Success = false,
                Errors = ["User with this email already exists"]
            };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.CreateAsync(user);

        _logger.LogInformation("Successfully created new user: {@User}", request);
        return new SignInResponse
        {
            Success = true
        };
    }
}