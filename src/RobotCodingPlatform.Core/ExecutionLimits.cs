namespace RobotCodingPlatform.Core;

/// <summary>
/// Ограничения на выполнение пользовательской программы робота.
/// </summary>
public sealed record ExecutionLimits
{
    public static ExecutionLimits Default { get; } = new();

    /// <summary>Максимальное количество узлов AST, выполненных интерпретатором.</summary>
    public int MaxIterations { get; init; } = 10_000;

    /// <summary>Максимальное суммарное количество шагов робота по всем командам move.</summary>
    public int MaxSteps { get; init; } = 10_000;

    /// <summary>Максимальное время выполнения программы.</summary>
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Выбрасывается, когда программа превышает лимиты выполнения.
/// </summary>
public sealed class ExecutionLimitExceededException(string message) : Exception(message);
