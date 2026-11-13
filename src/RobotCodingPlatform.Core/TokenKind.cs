namespace RobotCodingPlatform.Core;

public enum TokenKind
{
    Command,
    Number,
    Operator,
    LParen, 
    RParen,
    EndOfLine,
    Indent,
    Dedent
}