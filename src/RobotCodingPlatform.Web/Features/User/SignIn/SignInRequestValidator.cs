using FluentValidation;

namespace RobotCodingPlatform.Web.Features.User.SignIn;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is necessary")
            .MinimumLength(2)
                .WithMessage("Name must contain at least 2 symbols")
            .MaximumLength(50)
                .WithMessage("Name length must be less than 50 symbols")
            .Matches(@"^[a-zA-Zа-яА-ЯёЁ0-9\s\-_']+$")
                .WithMessage("Name must contain only letters, digits, spaces, '-', '_' and '`'")
            .WithName("Имя");

        RuleFor(x => x.Email)
            .NotEmpty()
                .WithMessage("Email is necessary")
            .EmailAddress()
                .WithMessage("Incorrect email address")
            .MaximumLength(254)
                .WithMessage("Email must be less than 254 symbols")
            .WithName("Email");

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is necessary")
            .MinimumLength(8)
                .WithMessage("Password length must be greater than 8 symbols")
            .MaximumLength(100)
                .WithMessage("Password length must be less than 100 symbols")
            .Matches(@"[A-Z]+")
                .WithMessage("Password must contain at least 1 capital letter (A-Z)")
            .Matches(@"[a-z]+")
                .WithMessage("Password must contain at least 1 small letter (a-z)")
            .Matches(@"[0-9]+")
                .WithMessage("Password must contain at least 1 digit")
            .WithName("Пароль");
    }
}
