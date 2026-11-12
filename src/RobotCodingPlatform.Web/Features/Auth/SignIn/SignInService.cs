using FluentValidation;
using Microsoft.AspNetCore.Identity;
using RobotCodingPlatform.Web.Features.Users;

namespace RobotCodingPlatform.Web.Features.Auth.SignIn;

public interface ISignInService
{
    public Task<SignInResponse> SignInAsync(SignInRequest request, CancellationToken ct = default);
}

public class SignInService(
    ITokenService tokenService,
    ILogger<SignInService> logger,
    IPasswordHasher<User> passwordHasher,
    IValidator<SignInRequest> validator,
    IUserRepository userRepository) : ISignInService
{
    private readonly ILogger<SignInService> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher
        ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IUserRepository _userRepository = userRepository
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IValidator<SignInRequest> _validator = validator
        ?? throw new ArgumentNullException(nameof(validator));

    public async Task<SignInResponse> SignInAsync(SignInRequest request, CancellationToken ct = default)
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

        if (await _userRepository.ExistsWithEmailAsync(request.Email))
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

        var result = await tokenService.GenerateTokensAsync(user, ct);

        _logger.LogInformation("Successfully created new user: {@User}", request);

        return new SignInResponse
        {
            Success = true,
            NewAccessToken = result.NewAccessToken,
            NewRefreshToken = result.NewRefreshToken,
            User = new UserDto(user.Name)
        };
    }
}