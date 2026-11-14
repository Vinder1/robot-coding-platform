using System.Globalization;

namespace RobotCodingPlatform.Core;

public static class Lexer
{
    private const int TabWidth = 4;

    public static List<Token> Tokenize(string text)
    {
        text = text.ToLowerInvariant();
        var tokens = new List<Token>();
        var indents = new Stack<int>();
        indents.Push(0);

        var lines = text.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains('#'))
                line = line[..line.IndexOf('#')];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var position = 0;
            var indentWidth = 0;

            while (position < line.Length && (line[position] == ' ' || line[position] == '\t'))
            {
                if (line[position] == ' ')
                    indentWidth++;
                else
                    indentWidth += TabWidth;

                position++;
            }

            if (indentWidth % TabWidth != 0)
                throw new SyntaxError("Incorrect indentation", i + 1);

            if (indentWidth > indents.Peek())
            {
                indents.Push(indentWidth);
                tokens.Add(new MarkerToken(TokenKind.Indent, i + 1));
            }
            else if (indentWidth < indents.Peek())
            {
                while (indents.Count > 1 && indentWidth < indents.Peek())
                {
                    indents.Pop();
                    tokens.Add(new MarkerToken(TokenKind.Dedent, i + 1));
                }

                if (indentWidth != indents.Peek())
                    throw new SyntaxError("Incorrect indentation", i + 1);
            }

            while (position < line.Length)
            {
                if (char.IsWhiteSpace(line[position]))
                {
                    position++;
                    continue;
                }

                var start = position;
                var current = line[position];

                if (char.IsLetter(current) || current == '_')
                {
                    position++;
                    while (position < line.Length &&
                           (char.IsLetterOrDigit(line[position]) || line[position] == '_'))
                        position++;

                    tokens.Add(new CommandToken(line[start..position], i + 1));
                }
                else if (char.IsDigit(current))
                {
                    position++;
                    while (position < line.Length && char.IsDigit(line[position]))
                        position++;

                    var text2 = line[start..position];
                    if (!int.TryParse(text2, NumberStyles.Integer,
                                      CultureInfo.InvariantCulture, out var value))
                        throw new SyntaxError($"Incorrect number '{text2}'", i + 1);

                    tokens.Add(new NumberToken(value, i + 1));
                }
                else if (current == '(')
                {
                    tokens.Add(new MarkerToken(TokenKind.LParen, i + 1));
                    position++;
                }
                else if (current == ')')
                {
                    tokens.Add(new MarkerToken(TokenKind.RParen, i + 1));
                    position++;
                }
                else if (current is '<' or '>')
                {
                    position++;
                    if (position < line.Length && line[position] == '=')
                        position++;

                    tokens.Add(new OperatorToken(line[start..position], i + 1));
                }
                else if (current == '=')
                {
                    position++;
                    if (position < line.Length && line[position] == '=')
                    {
                        position++;
                        tokens.Add(new OperatorToken("==", i + 1));
                    }
                    else
                    {
                        throw new SyntaxError("Expected '==', got '='", i + 1);
                    }
                }
                else if (current == '!')
                {
                    position++;
                    if (position < line.Length && line[position] == '=')
                    {
                        position++;
                        tokens.Add(new OperatorToken("!=", i + 1));
                    }
                    else
                    {
                        throw new SyntaxError("Expected '!=', got '!'", i + 1);
                    }
                }
                else
                {
                    throw new SyntaxError($"Unexpected symbol '{current}'", i + 1);
                }
            }

            tokens.Add(new MarkerToken(TokenKind.EndOfLine, i + 1));
        }

        while (indents.Count > 1)
        {
            indents.Pop();
            tokens.Add(new MarkerToken(TokenKind.Dedent, lines.Length));
        }

        return tokens;
    }
}