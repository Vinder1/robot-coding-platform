using System.Diagnostics;

namespace RobotCodingPlatform.Core;

public class Interpreter(IRobot robot, ExecutionLimits? limits = null)
{
    private readonly ExecutionLimits _limits = limits ?? ExecutionLimits.Default;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _iterations;

    public int Iterations => _iterations;

    public void Interpret(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes) ExecuteNode(node);
    }

    private void ExecuteNode(Node node)
    {
        CheckBudget();

        switch (node)
        {
            case ActionCommand actionCommand:
                robot.ExecuteCommand(actionCommand.Name, actionCommand.Arguments);
                break;

            case IfStatement ifs:
                if (EvaluateCondition(ifs.Condition)) 
                    Interpret(ifs.Children);
                else if (ifs.ElseChildren is not null) 
                    Interpret(ifs.ElseChildren);
                break;

            case WhileStatement whileStatement:
                while (EvaluateCondition(whileStatement.Condition))
                {
                    CheckBudget();
                    Interpret(whileStatement.Children);
                }
                break;

            default:
                throw new InvalidOperationException($"Unknown AST node type: {node.GetType().Name}");
        }
    }

    private void CheckBudget()
    {
        _iterations++;

        if (_iterations > _limits.MaxIterations)
            throw new ExecutionLimitExceededException(
                $"Execution limit exceeded: more than {_limits.MaxIterations} iterations.");

        if (_stopwatch.Elapsed > _limits.MaxDuration)
            throw new ExecutionLimitExceededException(
                $"Execution limit exceeded: time limit of {_limits.MaxDuration.TotalSeconds:0.###} seconds.");
    }

    private bool EvaluateCondition(Condition? condition)
    {
        return condition switch
        {
            BoolLiteral literal => literal.Value,
            SensorCondition sensor => robot.GetSensor(sensor.Name),
            AndCondition and => EvaluateCondition(and.Left) && EvaluateCondition(and.Right),
            OrCondition or => EvaluateCondition(or.Left) || EvaluateCondition(or.Right),
            NotCondition not => !EvaluateCondition(not.Inner),
            _ => throw new InvalidOperationException("Condition is missing or unknown")
        };
    }
}
