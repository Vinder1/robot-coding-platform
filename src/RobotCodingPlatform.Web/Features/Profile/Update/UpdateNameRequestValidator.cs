using FluentValidation;

namespace RobotCodingPlatform.Web.Features.Profile.Update;

public class UpdateNameRequestValidator : AbstractValidator<UpdateNameRequest>
{
    public UpdateNameRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
                .WithMessage("Name is necessary")
            .NotEmpty()
                .WithMessage("Name is necessary")
            .MinimumLength(2)
                .WithMessage("Name must contain at least 2 symbols")
            .MaximumLength(50)
                .WithMessage("Name length must be less than 50 symbols")
            .Matches(@"^[a-zA-Zа-яА-ЯёЁ0-9\s\-_']+$")
                .WithMessage("Name must contain only letters, digits, spaces, '-', '_' and '`'")
            .WithName("Имя");
    }
}
