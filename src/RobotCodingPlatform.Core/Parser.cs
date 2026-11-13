namespace RobotCodingPlatform.Core;

public static class Parser
{
    private static readonly HashSet<string> Keywords =
        ["if", "else", "while", "and", "or", "not", "true", "false"];

    public static List<Node> Parse(IReadOnlyList<Token> tokens)
    {
        var position = 0;
        var result = ParseBlock(tokens, ref position);

        if (position < tokens.Count)
            throw new SyntaxError($"Unexpected token '{tokens[position]}'", tokens[position].Line);

        return result;
    }

    private static List<Node> ParseBlock(IReadOnlyList<Token> tokens, ref int position)
    {
        var result = new List<Node>();

        while (position < tokens.Count)
        {
            var token = tokens[position];

            switch (token)
            {
                case CommandToken commandToken:
                    position++;
                    result.Add(ParseCommandNode(tokens, ref position, commandToken));
                    break;

                case MarkerToken { Kind: TokenKind.Indent } indentToken:
                    if (result.Count == 0)
                        throw new SyntaxError("Unexpected indent", indentToken.Line);

                    var parent = result[^1];
                    if (parent is not (IfStatement or WhileStatement))
                        throw new SyntaxError("This statement cannot have a nested block", indentToken.Line);

                    position++;
                    var children = ParseBlock(tokens, ref position);

                    result[^1] = parent switch
                    {
                        IfStatement ifs => ifs with { Children = children },
                        WhileStatement wLoop => wLoop with { Children = children },
                        _ => parent
                    };

                    if (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.Dedent })
                        throw new SyntaxError("Block is not closed", indentToken.Line);

                    position++;

                    if (result[^1] is IfStatement ifNode &&
                        position < tokens.Count &&
                        tokens[position] is CommandToken { Text: "else" } elseToken)
                    {
                        position++;

                        if (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.EndOfLine })
                            throw new SyntaxError("Expected end of line after 'else'", elseToken.Line);
                        position++;

                        if (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.Indent })
                            throw new SyntaxError("'else' requires an indented block", elseToken.Line);
                        position++;

                        var elseChildren = ParseBlock(tokens, ref position);

                        if (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.Dedent })
                            throw new SyntaxError("'else' block is not closed", elseToken.Line);
                        position++;

                        result[^1] = ifNode with { ElseChildren = elseChildren };
                    }
                    break;

                case MarkerToken { Kind: TokenKind.Dedent }:
                    return result;

                case MarkerToken markerToken:
                    throw new SyntaxError($"Unexpected '{markerToken.Kind}'", markerToken.Line);

                case NumberToken numberToken:
                    throw new SyntaxError($"Unexpected number '{numberToken.Number}'", numberToken.Line);

                case OperatorToken operatorToken:
                    throw new SyntaxError($"Unexpected operator '{operatorToken.Text}'", operatorToken.Line);

                default:
                    throw new SyntaxError($"Unexpected token '{token}'", token.Line);
            }
        }

        return result;
    }

    private static Node ParseCommandNode(IReadOnlyList<Token> tokens, ref int position, CommandToken keyword)
    {
        var name = keyword.Text;
        var line = keyword.Line;
        var isConditional = name is "if" or "while";

        if (!isConditional && Keywords.Contains(name))
            throw new SyntaxError($"'{name}' cannot be used as a command", line);

        Condition? condition = null;
        if (isConditional)
            condition = ParseCondition(tokens, ref position);

        var arguments = new List<int>();
        var endOfLine = false;

        while (!endOfLine && position < tokens.Count)
        {
            switch (tokens[position])
            {
                case NumberToken numberToken:
                    if (isConditional)
                        throw new SyntaxError($"Unexpected argument '{numberToken.Number}' after condition", numberToken.Line);
                    arguments.Add(numberToken.Number);
                    position++;
                    break;

                case MarkerToken { Kind: TokenKind.EndOfLine }:
                    endOfLine = true;
                    position++;
                    break;

                case CommandToken extra:
                    throw new SyntaxError($"Expected end of line, got '{extra.Text}'", extra.Line);

                case OperatorToken op:
                    throw new SyntaxError($"Unexpected operator '{op.Text}'", op.Line);

                case MarkerToken m:
                    throw new SyntaxError($"Unexpected '{m.Kind}'", m.Line);

                default:
                    throw new SyntaxError($"Unexpected token '{tokens[position]}'", tokens[position].Line);
            }
        }

        if (!endOfLine)
            throw new SyntaxError("Expected end of line, got end of input", line);

        if (isConditional && (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.Indent }))
            throw new SyntaxError($"'{name}' requires an indented block", line);

        return name switch
        {
            "if" => new IfStatement(condition!, [], null, line),
            "while" => new WhileStatement(condition!, [], line),
            _ => new ActionCommand(name, arguments, line)
        };
    }

    private static Condition ParseCondition(IReadOnlyList<Token> tokens, ref int position)
        => ParseOr(tokens, ref position);

    private static Condition ParseOr(IReadOnlyList<Token> tokens, ref int position)
    {
        var left = ParseAnd(tokens, ref position);

        while (position < tokens.Count && tokens[position] is CommandToken { Text: "or" })
        {
            position++;
            left = new OrCondition(left, ParseAnd(tokens, ref position));
        }

        return left;
    }

    private static Condition ParseAnd(IReadOnlyList<Token> tokens, ref int position)
    {
        var left = ParseNot(tokens, ref position);

        while (position < tokens.Count && tokens[position] is CommandToken { Text: "and" })
        {
            position++;
            left = new AndCondition(left, ParseNot(tokens, ref position));
        }

        return left;
    }

    private static Condition ParseNot(IReadOnlyList<Token> tokens, ref int position)
    {
        if (position < tokens.Count && tokens[position] is CommandToken { Text: "not" })
        {
            position++;
            return new NotCondition(ParseNot(tokens, ref position));
        }

        return ParsePrimary(tokens, ref position);
    }

    private static Condition ParsePrimary(IReadOnlyList<Token> tokens, ref int position)
    {
        if (position >= tokens.Count)
            throw new SyntaxError("Expected condition, got end of input", tokens.Count > 0 ? tokens[^1].Line : 1);

        var token = tokens[position];

        if (token is MarkerToken { Kind: TokenKind.LParen } lparen)
        {
            position++;
            var inner = ParseCondition(tokens, ref position);

            if (position >= tokens.Count || tokens[position] is not MarkerToken { Kind: TokenKind.RParen })
                throw new SyntaxError("Expected ')'", position < tokens.Count ? tokens[position].Line : lparen.Line);

            position++;
            return inner;
        }

        if (token is not CommandToken commandToken)
            throw new SyntaxError($"Expected condition, got '{token}'", token.Line);

        if (commandToken.Text is "true" or "false")
        {
            position++;
            return new BoolLiteral(commandToken.Text == "true");
        }

        if (Keywords.Contains(commandToken.Text))
            throw new SyntaxError($"Unexpected keyword '{commandToken.Text}'", commandToken.Line);

        position++;
        return new SensorCondition(commandToken.Text);
    }
}