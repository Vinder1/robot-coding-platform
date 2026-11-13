namespace RobotCodingPlatform.Core;

public abstract record Token(TokenKind Kind, int Line);

public sealed record CommandToken(string Text, int Line) : Token(TokenKind.Command, Line);

public sealed record OperatorToken(string Text, int Line) : Token(TokenKind.Operator, Line);

public sealed record NumberToken(int Number, int Line) : Token(TokenKind.Number, Line);

public sealed record MarkerToken(TokenKind Kind, int Line) : Token(Kind, Line);