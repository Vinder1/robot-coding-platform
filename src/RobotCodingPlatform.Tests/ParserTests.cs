using RobotCodingPlatform.Core;

namespace RobotCodingPlatform.Tests;

public class ParserTests
{
    private static List<Node> Parse(string source) => Parser.Parse(Lexer.Tokenize(source));

    [Fact]
    public void Parse_EmptyInput_ReturnsEmptyTree()
    {
        Assert.Empty(Parse(""));
    }

    [Fact]
    public void Parse_SingleCommandWithoutArguments()
    {
        var tree = Parse("stop");

        var node = Assert.Single(tree);
        var action = Assert.IsType<ActionCommand>(node);
        Assert.Equal("stop", action.Name);
        Assert.Empty(action.Arguments);
    }

    [Fact]
    public void Parse_TwoCommandsAtSameLevel()
    {
        var tree = Parse("stop\nhalt");

        Assert.Equal(2, tree.Count);
        Assert.Equal("stop", Assert.IsType<ActionCommand>(tree[0]).Name);
        Assert.Equal("halt", Assert.IsType<ActionCommand>(tree[1]).Name);
    }

    [Fact]
    public void Parse_WhitespaceOnly_ReturnsEmptyTree()
    {
        Assert.Empty(Parse("   \n\n\t\n"));
    }

    [Fact]
    public void Parse_CommentsOnly_ReturnsEmptyTree()
    {
        Assert.Empty(Parse("# nothing here\n# and here"));
    }

    [Fact]
    public void Parse_SingleCommandWithOneArgument()
    {
        var tree = Parse("move 10");

        var node = Assert.Single(tree);
        var action = Assert.IsType<ActionCommand>(node);
        Assert.Equal("move", action.Name);
        Assert.Equal(new[] { 10 }, action.Arguments);
    }

    [Fact]
    public void Parse_SingleCommandWithThreeArguments()
    {
        var tree = Parse("move 1 2 3");

        var action = Assert.IsType<ActionCommand>(Assert.Single(tree));
        Assert.Equal(new[] { 1, 2, 3 }, action.Arguments);
    }

    [Fact]
    public void Parse_CommandNameIsLowercased()
    {
        var tree = Parse("MOVE 10");

        Assert.Equal("move", Assert.IsType<ActionCommand>(tree[0]).Name);
    }

    [Fact]
    public void Parse_NumberPreservesParsedValueNotRawText()
    {
        var tree = Parse("move 007");

        Assert.Equal(new[] { 7 }, Assert.IsType<ActionCommand>(tree[0]).Arguments);
    }

    [Fact]
    public void Parse_TwoCommandsOnSeparateLines()
    {
        var tree = Parse("stop\nhalt");

        Assert.Equal(2, tree.Count);
        Assert.Equal("stop", Assert.IsType<ActionCommand>(tree[0]).Name);
        Assert.Equal("halt", Assert.IsType<ActionCommand>(tree[1]).Name);
    }

    [Fact]
    public void Parse_ThreeCommandsWithMixedArguments()
    {
        var tree = Parse(
            "move 10\n" +
            "turn 90\n" +
            "wait 5");

        Assert.Equal(3, tree.Count);
        Assert.Equal(new[] { 10 }, Assert.IsType<ActionCommand>(tree[0]).Arguments);
        Assert.Equal(new[] { 90 }, Assert.IsType<ActionCommand>(tree[1]).Arguments);
        Assert.Equal(new[] { 5 }, Assert.IsType<ActionCommand>(tree[2]).Arguments);
    }

    [Fact]
    public void Parse_BlankLinesBetweenCommandsAreIgnored()
    {
        var tree = Parse(
            "move 10\n" +
            "\n" +
            "   \n" +
            "turn 90");

        Assert.Equal(2, tree.Count);
    }

    [Fact]
    public void Parse_CommentsBetweenCommandsAreIgnored()
    {
        var tree = Parse(
            "move 10  # go forward\n" +
            "# a full-line comment\n" +
            "turn 90");

        Assert.Equal(2, tree.Count);
    }

    [Fact]
    public void Parse_CommandWithTrailingNewline()
    {
        Assert.Single(Parse("stop\n"));
    }

    [Fact]
    public void Parse_CommandWithTrailingBlankLines()
    {
        Assert.Single(Parse("stop\n\n\n"));
    }

    [Fact]
    public void Parse_TwoCommandsOnSameLine_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("move stop"));
    }

    [Fact]
    public void Parse_NumberAtStartOfStatement_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("10 move"));
    }

    [Fact]
    public void Parse_LoneNumberOnNextLine_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("move 10\n20"));
    }

    [Fact]
    public void Parse_NumberTooLargeForInt_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("move 99999999999999999999"));
    }

    [Fact]
    public void Parse_KeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if"));
    }

    [Fact]
    public void Parse_ElseAtTopLevel_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("else"));
    }

    [Fact]
    public void Parse_LeadingIndent_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("    move 10"));
    }

    [Fact]
    public void Parse_NonIfWhileIndent_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("move 10\n    turn 90"));
    }

    [Fact]
    public void Parse_IfWithoutIndentedBlock_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if true\nstop"));
    }

    [Fact]
    public void Parse_IfTrueBlock()
    {
        var tree = Parse("if true\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        var cond = Assert.IsType<BoolLiteral>(ifs.Condition);
        Assert.True(cond.Value);
        var child = Assert.IsType<ActionCommand>(Assert.Single(ifs.Children));
        Assert.Equal("stop", child.Name);
    }

    [Fact]
    public void Parse_IfFalseBlock()
    {
        var tree = Parse("if false\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        var cond = Assert.IsType<BoolLiteral>(ifs.Condition);
        Assert.False(cond.Value);
    }

    [Fact]
    public void Parse_IfSensorCondition()
    {
        var tree = Parse("if obstacle\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        var cond = Assert.IsType<SensorCondition>(ifs.Condition);
        Assert.Equal("obstacle", cond.Name);
    }

    [Fact]
    public void Parse_IfComplexConditionAnd()
    {
        var tree = Parse("if a and b\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        Assert.IsType<AndCondition>(ifs.Condition);
    }

    [Fact]
    public void Parse_IfComplexConditionOr()
    {
        var tree = Parse("if a or b\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        Assert.IsType<OrCondition>(ifs.Condition);
    }

    [Fact]
    public void Parse_IfComplexConditionNot()
    {
        var tree = Parse("if not a\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        var cond = Assert.IsType<NotCondition>(ifs.Condition);
        Assert.IsType<SensorCondition>(cond.Inner);
    }

    [Fact]
    public void Parse_IfNestedNotAndOr()
    {
        var tree = Parse("if not (a and b) or c\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        Assert.NotNull(ifs.Condition);
    }

    [Fact]
    public void Parse_ConditionUnexpectedKeyword_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if if\n    stop"));
    }

    [Fact]
    public void Parse_ConditionMissingRightParen_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if (a\n    stop"));
    }

    [Fact]
    public void Parse_EmptyCondition_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if\n    stop"));
    }

    [Fact]
    public void Parse_IfWithArguments_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if true 10\n    stop"));
    }

    [Fact]
    public void Parse_WhileLoop()
    {
        var tree = Parse("while true\n    move 1");
        var node = Assert.Single(tree);
        var wLoop = Assert.IsType<WhileStatement>(node);
        Assert.Single(wLoop.Children);
    }

    [Fact]
    public void Parse_NestedIfBlocks()
    {
        var tree = Parse("if true\n    if false\n        stop");
        var outer = Assert.IsType<IfStatement>(Assert.Single(tree));
        var inner = Assert.IsType<IfStatement>(Assert.Single(outer.Children));
        Assert.Single(inner.Children);
        var action = Assert.IsType<ActionCommand>(inner.Children[0]);
        Assert.Equal("stop", action.Name);
    }

    [Fact]
    public void Parse_MultipleCommandsInBlock()
    {
        var tree = Parse("if true\n    move 10\n    turn 90\n    stop");
        var outer = Assert.IsType<IfStatement>(Assert.Single(tree));
        Assert.Equal(3, outer.Children.Count);
    }

    [Fact]
    public void Parse_ExtraCommandTokenOnSameLine_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("move 10 turn 90"));
    }

    [Fact]
    public void Parse_AndKeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("and"));
    }

    [Fact]
    public void Parse_OrKeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("or"));
    }

    [Fact]
    public void Parse_NotKeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("not"));
    }

    [Fact]
    public void Parse_TrueKeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("true"));
    }

    [Fact]
    public void Parse_FalseKeywordAsCommand_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("false"));
    }

    [Fact]
    public void Parse_UnmatchedRightParenInCondition_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if a)\n    stop"));
    }

    [Fact]
    public void Parse_DoubleNotCondition()
    {
        var tree = Parse("if not not a\n    stop");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);
        var not1 = Assert.IsType<NotCondition>(ifs.Condition);
        var not2 = Assert.IsType<NotCondition>(not1.Inner);
        Assert.IsType<SensorCondition>(not2.Inner);
    }

    [Fact]
    public void Parse_OperatorTokenAtTopLevel_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("=="));
    }

    [Fact]
    public void Parse_NumberTokenAtTopLevel_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("10"));
    }

    [Fact]
    public void Parse_IfElseBlock()
    {
        var tree = Parse("if true\n    stop\nelse\n    move 10");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);

        var ifChild = Assert.IsType<ActionCommand>(Assert.Single(ifs.Children));
        Assert.Equal("stop", ifChild.Name);

        Assert.NotNull(ifs.ElseChildren);
        var elseChild = Assert.IsType<ActionCommand>(Assert.Single(ifs.ElseChildren));
        Assert.Equal("move", elseChild.Name);
        Assert.Equal(new[] { 10 }, elseChild.Arguments);
    }

    [Fact]
    public void Parse_IfElseWithMultipleCommands()
    {
        var tree = Parse("if obstacle\n    stop\nelse\n    move 5\n    turn 90");
        var node = Assert.Single(tree);
        var ifs = Assert.IsType<IfStatement>(node);

        Assert.NotNull(ifs.ElseChildren);
        Assert.Equal(2, ifs.ElseChildren.Count);
        Assert.Equal("move", Assert.IsType<ActionCommand>(ifs.ElseChildren[0]).Name);
        Assert.Equal("turn", Assert.IsType<ActionCommand>(ifs.ElseChildren[1]).Name);
    }

    [Fact]
    public void Parse_NestedIfElse()
    {
        var tree = Parse("if a\n    move 1\nelse\n    if b\n        move 2\n    else\n        move 3");
        var node = Assert.Single(tree);
        var outerIf = Assert.IsType<IfStatement>(node);

        Assert.NotNull(outerIf.ElseChildren);
        var innerIf = Assert.IsType<IfStatement>(Assert.Single(outerIf.ElseChildren));

        Assert.NotNull(innerIf.ElseChildren);
        var innerElseChild = Assert.IsType<ActionCommand>(Assert.Single(innerIf.ElseChildren));
        Assert.Equal(new[] { 3 }, innerElseChild.Arguments);
    }

    [Fact]
    public void Parse_ElseWithoutIndentedBlock_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if true\n    stop\nelse\nstop"));
    }

    [Fact]
    public void Parse_ElseWithoutIf_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("else\n    stop"));
    }

    [Fact]
    public void Parse_ElseWithArguments_Throws()
    {
        Assert.Throws<SyntaxError>(() => Parse("if true\n    stop\nelse 10\n    move 5"));
    }
}