using FluentValidation;

namespace RobotCodingPlatform.Web.Features.Solution.Create;

public class CreateSolutionAttemptRequestValidator : AbstractValidator<CreateSolutionAttemptRequest>
{
    public const int MaxCodeLength = 20_000;

    public CreateSolutionAttemptRequestValidator()
    {
        RuleFor(x => x.ChallengeId)
            .NotEmpty()
                .WithMessage("Не указано испытание.");

        RuleFor(x => x.Code)
            .NotEmpty()
                .WithMessage("Код решения не может быть пустым.")
            .MaximumLength(MaxCodeLength)
                .WithMessage($"Код решения не должен превышать {MaxCodeLength} символов.");
    }
}
