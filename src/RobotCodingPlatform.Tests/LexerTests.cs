using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_RecognizesCommandAndNumber()
    {
        var tokens = Lexer.Tokenize("MOVE 10");

        Assert.Equal(
            new[]
            {
                (TokenKind.Command, "move", 1),
                (TokenKind.Number, "10", 1),
                (TokenKind.EndOfLine, null, 1)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_EmitsIndentAndDedent()
    {
        var tokens = Lexer.Tokenize(
            "repeat 2\n" +
            "    move 10\n" +
            "stop");

        Assert.Equal(
            new[]
            {
                (TokenKind.Command, "repeat", 1),
                (TokenKind.Number, "2", 1),
                (TokenKind.EndOfLine, null, 1),

                (TokenKind.Indent, null, 2),
                (TokenKind.Command, "move", 2),
                (TokenKind.Number, "10", 2),
                (TokenKind.EndOfLine, null, 2),

                (TokenKind.Dedent, null, 3),
                (TokenKind.Command, "stop", 3),
                (TokenKind.EndOfLine, null, 3)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_ThrowsOnUnexpectedCharacter()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("move@"));
    }

    [Fact]
    public void Tokenize_ThrowsOnIndentThatIsNotMultipleOfTabWidth()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("move\n  turn_left"));
    }

    [Fact]
    public void Tokenize_IgnoresCommentsAndBlankLines()
    {
        var tokens = Lexer.Tokenize(
            "MOVE 3 # comment\n" +
            "\n" +
            "# comment\n" +
            "TURN_LEFT");

        Assert.Equal(
            new[]
            {
                (TokenKind.Command, "move", 1),
                (TokenKind.Number, "3", 1),
                (TokenKind.EndOfLine, null, 1),
                (TokenKind.Command, "turn_left", 4),
                (TokenKind.EndOfLine, null, 4)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_EmitsIndentAndDedentForNestedBlocks()
    {
        var tokens = Lexer.Tokenize(
            "root\n" +
            "    if\n" +
            "        move 1\n" +
            "    turn_left\n" +
            "stop");

        Assert.Equal(
            new[]
            {
                (TokenKind.Command, "root", 1),
                (TokenKind.EndOfLine, null, 1),

                (TokenKind.Indent, null, 2),
                (TokenKind.Command, "if", 2),
                (TokenKind.EndOfLine, null, 2),

                (TokenKind.Indent, null, 3),
                (TokenKind.Command, "move", 3),
                (TokenKind.Number, "1", 3),
                (TokenKind.EndOfLine, null, 3),

                (TokenKind.Dedent, null, 4),
                (TokenKind.Command, "turn_left", 4),
                (TokenKind.EndOfLine, null, 4),

                (TokenKind.Dedent, null, 5),
                (TokenKind.Command, "stop", 5),
                (TokenKind.EndOfLine, null, 5)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_AllowsDigitsAfterFirstIdentifierCharacter()
    {
        var tokens = Lexer.Tokenize("m0ve");

        Assert.Equal(TokenKind.Command, tokens[0].Kind);
        Assert.Equal("m0ve", ((CommandToken)tokens[0]).Text);
    }

    [Fact]
    public void Tokenize_RecognizesParentheses()
    {
        var tokens = Lexer.Tokenize("(a)");

        Assert.Equal(
            new[]
            {
                (TokenKind.LParen, null, 1),
                (TokenKind.Command, "a", 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.EndOfLine, null, 1)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_RecognizesParenthesesGluedToIdentifiers()
    {
        var tokens = Lexer.Tokenize("(a)and(b)");

        Assert.Equal(
            new[]
            {
                (TokenKind.LParen, null, 1),
                (TokenKind.Command, "a", 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.Command, "and", 1),
                (TokenKind.LParen, null, 1),
                (TokenKind.Command, "b", 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.EndOfLine, null, 1)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_RecognizesNestedParentheses()
    {
        var tokens = Lexer.Tokenize("((a))");

        Assert.Equal(
            new[]
            {
                (TokenKind.LParen, null, 1),
                (TokenKind.LParen, null, 1),
                (TokenKind.Command, "a", 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.EndOfLine, null, 1)
            },
            Simplify(tokens));
    }

    [Theory]
    [InlineData("a < b", "<")]
    [InlineData("a > b", ">")]
    [InlineData("a <= b", "<=")]
    [InlineData("a >= b", ">=")]
    [InlineData("a == b", "==")]
    [InlineData("a != b", "!=")]
    public void Tokenize_RecognizesOperatorWithSpaces(string input, string expected)
    {
        var tokens = Lexer.Tokenize(input);

        Assert.Equal(TokenKind.Operator, tokens[1].Kind);
        Assert.Equal(expected, ((OperatorToken)tokens[1]).Text);
    }

    [Theory]
    [InlineData("a<b", "<")]
    [InlineData("a>b", ">")]
    [InlineData("a<=b", "<=")]
    [InlineData("a>=b", ">=")]
    [InlineData("a==b", "==")]
    [InlineData("a!=b", "!=")]
    public void Tokenize_RecognizesOperatorWithoutSpaces(string input, string expected)
    {
        var tokens = Lexer.Tokenize(input);

        Assert.Equal(TokenKind.Operator, tokens[1].Kind);
        Assert.Equal(expected, ((OperatorToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenize_LessOrEqualIsSingleToken()
    {
        var tokens = Lexer.Tokenize("a<=b");

        Assert.Equal(4, tokens.Count);
        Assert.Equal(TokenKind.Operator, tokens[1].Kind);
        Assert.Equal("<=", ((OperatorToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenize_GreaterOrEqualIsSingleToken()
    {
        var tokens = Lexer.Tokenize("a>=b");

        Assert.Equal(4, tokens.Count);
        Assert.Equal(">=", ((OperatorToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenize_DoubleEqualsIsSingleToken()
    {
        var tokens = Lexer.Tokenize("a==b");

        Assert.Equal(4, tokens.Count);
        Assert.Equal("==", ((OperatorToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenize_NotEqualIsSingleToken()
    {
        var tokens = Lexer.Tokenize("a!=b");

        Assert.Equal(4, tokens.Count);
        Assert.Equal("!=", ((OperatorToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenize_RecognizesMultipleOperatorsInARow()
    {
        var tokens = Lexer.Tokenize("a < b < c");

        Assert.Equal("<", ((OperatorToken)tokens[1]).Text);
        Assert.Equal("<", ((OperatorToken)tokens[3]).Text);
    }

    [Fact]
    public void Tokenize_AdjacentLessAndGreaterAreTwoOperators()
    {
        var tokens = Lexer.Tokenize("a<>b");

        Assert.Equal(TokenKind.Operator, tokens[1].Kind);
        Assert.Equal("<", ((OperatorToken)tokens[1]).Text);
        Assert.Equal(TokenKind.Operator, tokens[2].Kind);
        Assert.Equal(">", ((OperatorToken)tokens[2]).Text);
    }

    [Fact]
    public void Tokenize_RecognizesMixedExpression()
    {
        var tokens = Lexer.Tokenize("(a or b) and c > 5");

        Assert.Equal(
            new[]
            {
                (TokenKind.LParen, null, 1),
                (TokenKind.Command, "a", 1),
                (TokenKind.Command, "or", 1),
                (TokenKind.Command, "b", 1),
                (TokenKind.RParen, null, 1),
                (TokenKind.Command, "and", 1),
                (TokenKind.Command, "c", 1),
                (TokenKind.Operator, ">", 1),
                (TokenKind.Number, "5", 1),
                (TokenKind.EndOfLine, null, 1)
            },
            Simplify(tokens));
    }

    [Fact]
    public void Tokenize_RecognizesExpressionOnIndentedLine()
    {
        var tokens = Lexer.Tokenize(
            "if\n" +
            "    (a or b) and c >= 5");

        Assert.Contains(tokens, t => t.Kind == TokenKind.Indent);
        Assert.Contains(tokens, t => t.Kind == TokenKind.LParen);
        Assert.Contains(tokens, t => t is OperatorToken { Text: ">=" });
    }

    [Fact]
    public void Tokenize_ThrowsOnSingleEquals()
    {
        var ex = Assert.Throws<SyntaxError>(() => Lexer.Tokenize("a = b"));
        Assert.Contains("==", ex.Message);
    }

    [Fact]
    public void Tokenize_ThrowsOnSingleEqualsAtEndOfLine()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("a ="));
    }

    [Fact]
    public void Tokenize_ThrowsOnBareEquals()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("="));
    }

    [Fact]
    public void Tokenize_ThrowsOnBareExclamation()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("!"));
    }

    [Fact]
    public void Tokenize_ThrowsOnExclamationAtEndOfLine()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("a!"));
    }

    [Fact]
    public void Tokenize_ReturnsEmptyForEmptyInput()
    {
        Assert.Empty(Lexer.Tokenize(""));
    }

    [Fact]
    public void Tokenize_ReturnsEmptyForWhitespaceOnly()
    {
        Assert.Empty(Lexer.Tokenize("   \n\n\t\n   "));
    }

    [Fact]
    public void Tokenize_ReturnsEmptyForCommentsOnly()
    {
        Assert.Empty(Lexer.Tokenize("# just a comment\n# another"));
    }

    [Fact]
    public void Tokenize_NumberTokenStoresParsedInt()
    {
        var tokens = Lexer.Tokenize("move 42");

        var num = Assert.IsType<NumberToken>(tokens[1]);
        Assert.Equal(42, num.Number);
    }

    [Fact]
    public void Tokenize_NumberTokenStripsLeadingZeros()
    {
        var tokens = Lexer.Tokenize("move 007");

        var num = Assert.IsType<NumberToken>(tokens[1]);
        Assert.Equal(7, num.Number);
    }

    [Fact]
    public void Tokenize_ThrowsOnNumberTooLargeForInt()
    {
        Assert.Throws<SyntaxError>(() => Lexer.Tokenize("move 99999999999999999999"));
    }

    private static (TokenKind Kind, string? Value, int Line)[] Simplify(IEnumerable<Token> tokens)
    {
        return tokens
            .Select(token => (token.Kind, GetText(token), token.Line))
            .ToArray();
    }

    private static string? GetText(Token token) => token switch
    {
        CommandToken c  => c.Text,
        OperatorToken o => o.Text,
        NumberToken n   => n.Number.ToString(),
        _               => null
    };
}