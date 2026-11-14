namespace RobotCodingPlatform.Core;

public abstract record Condition;

public sealed record SensorCondition(string Name) : Condition;

public sealed record BoolLiteral(bool Value) : Condition;

public sealed record AndCondition(Condition Left, Condition Right) : Condition;

public sealed record OrCondition(Condition Left, Condition Right) : Condition;

public sealed record NotCondition(Condition Inner) : Condition;