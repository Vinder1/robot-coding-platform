using FluentValidation;
using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Web.Features.Challenge.Create;

public class CreateChallengeRequestValidator : AbstractValidator<CreateChallengeRequest>
{
    public CreateChallengeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно.")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание обязательно.")
            .MaximumLength(10_000).WithMessage("Описание не должно превышать 10000 символов.");

        RuleFor(x => x.Width)
            .InclusiveBetween(1, 100).WithMessage("Ширина должна быть от 1 до 100.");

        RuleFor(x => x.Height)
            .InclusiveBetween(1, 100).WithMessage("Высота должна быть от 1 до 100.");

        RuleFor(x => x.StartX)
            .GreaterThanOrEqualTo(0).WithMessage("Стартовая координата X не может быть отрицательной.")
            .LessThan(x => x.Width).WithMessage("Стартовая точка должна находиться внутри карты.");

        RuleFor(x => x.StartY)
            .GreaterThanOrEqualTo(0).WithMessage("Стартовая координата Y не может быть отрицательной.")
            .LessThan(x => x.Height).WithMessage("Стартовая точка должна находиться внутри карты.");

        RuleFor(x => x.GoalX)
            .GreaterThanOrEqualTo(0).WithMessage("Координата цели X не может быть отрицательной.")
            .LessThan(x => x.Width).WithMessage("Цель должна находиться внутри карты.");

        RuleFor(x => x.GoalY)
            .GreaterThanOrEqualTo(0).WithMessage("Координата цели Y не может быть отрицательной.")
            .LessThan(x => x.Height).WithMessage("Цель должна находиться внутри карты.");

        RuleFor(x => x)
            .Must(x => x.GoalX != x.StartX || x.GoalY != x.StartY)
            .WithMessage("Цель не может совпадать со стартовой точкой.");

        RuleFor(x => x.Obstacles)
            .NotNull().WithMessage("Список препятствий обязателен.");

        RuleForEach(x => x.Obstacles)
            .Must((request, obstacle) =>
                obstacle.X >= 0 && obstacle.X < request.Width &&
                obstacle.Y >= 0 && obstacle.Y < request.Height)
            .WithMessage("Препятствие должно находиться внутри карты.");

        RuleFor(x => x.Obstacles)
            .Must(obstacles => obstacles != null && obstacles
                .Select(o => (o.X, o.Y))
                .Distinct()
                .Count() == obstacles.Count)
            .WithMessage("Препятствия не должны дублироваться.");

        RuleFor(x => x.Obstacles)
            .Must((request, obstacles) => obstacles == null || !obstacles
                .Any(o => o.X == request.StartX && o.Y == request.StartY))
            .WithMessage("Стартовая точка не может совпадать с препятствием.");

        RuleFor(x => x.Obstacles)
            .Must((request, obstacles) => obstacles == null || !obstacles
                .Any(o => o.X == request.GoalX && o.Y == request.GoalY))
            .WithMessage("Цель не может совпадать с препятствием.");

        RuleFor(x => x.Obstacles)
            .Must(obstacles => obstacles == null || obstacles.Count <= MapValidation.MaxObstacles)
            .WithMessage($"Количество препятствий не должно превышать {MapValidation.MaxObstacles}.");

        RuleFor(x => x)
            .Must(x => MapValidation.IsReachable(
                x.Width,
                x.Height,
                (x.StartX, x.StartY),
                (x.GoalX, x.GoalY),
                x.Obstacles.Select(o => (o.X, o.Y)).ToHashSet()))
            .WithMessage("Цель недостижима из стартовой точки.")
            .When(x => x.Width is > 0 and <= MapValidation.MaxDimension
                    && x.Height is > 0 and <= MapValidation.MaxDimension
                    && x.StartX >= 0 && x.StartX < x.Width
                    && x.StartY >= 0 && x.StartY < x.Height
                    && x.GoalX >= 0 && x.GoalX < x.Width
                    && x.GoalY >= 0 && x.GoalY < x.Height
                    && x.Obstacles is not null);
    }
}
