using MassTransit;
using RobotCodingPlatform.Core.Contracts;
using RobotCodingPlatform.Web.Features.Challenge;

namespace RobotCodingPlatform.Web.Features.Code;

public interface IRunCodeService
{
    public Task<RunCodeResult> Run(RunCodeParams parameters, CancellationToken cancellationToken);

    public Task<RunCodeResult?> RunChallenge(Guid challengeId, string code, CancellationToken cancellationToken);
}

public class RunCodeService(
    IRequestClient<RunScriptCommand> client,
    IChallengeRepository challengeRepository
) : IRunCodeService
{
    public async Task<RunCodeResult> Run(RunCodeParams parameters, CancellationToken cancellationToken)
    {
        var command = new RunScriptCommand(
                Code: parameters.Code,
                Width: parameters.Width,
                Height: parameters.Height,
                Goal: (parameters.Goal.X, parameters.Goal.Y),
                Obstacles: [.. parameters.Obstacles.Select(o => (o.X, o.Y))],
                StartX: parameters.Start.X,
                StartY: parameters.Start.Y
            );

        try
        {
            var response = await client.GetResponse<RunScriptResult>(
                command, cancellationToken, RequestTimeout.After(s: 10));

            return new RunCodeResult() { Success = response.Message.Success, Message = response.Message.Error ?? "", Compiled = true };
        }
        catch (RequestTimeoutException)
        {
            return new RunCodeResult() { Compiled = false, Success = false, Message = "Выполнение скрипта истекло по таймауту" };
        }
        catch (RequestFaultException ex)
        {
            return new RunCodeResult() { Compiled = false, Success = false, Message = $"Ошибка Worker: {ex.Message}" };
        }
    }

    public async Task<RunCodeResult?> RunChallenge(
        Guid challengeId,
        string code,
        CancellationToken cancellationToken)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId, cancellationToken);
        if (challenge is null)
        {
            return null;
        }

        var parameters = new RunCodeParams
        {
            Width = challenge.Width,
            Height = challenge.Height,
            Goal = new Point { X = challenge.GoalX, Y = challenge.GoalY },
            Start = new Point { X = challenge.StartX, Y = challenge.StartY },
            Obstacles = [.. challenge.Obstacles.Select(o => new Point { X = o.X, Y = o.Y })],
            Code = code
        };

        return await Run(parameters, cancellationToken);
    }
}
