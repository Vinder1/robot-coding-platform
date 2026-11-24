namespace RobotCodingPlatform.Web.Features.Auth;

public record UserDto(string Name, string Email, DateTime CreatedAt, Guid Id, string Role);
