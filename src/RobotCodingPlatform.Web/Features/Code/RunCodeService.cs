using MassTransit;
using RobotCodingPlatform.Core.Contracts;

namespace RobotCodingPlatform.Web.Features.Code;

public interface IRunCodeService
{
    public Task<RunCodeResult> Run(RunCodeParams parameters, CancellationToken cancellationToken);
}

public class RunCodeService(
    IRequestClient<RunScriptCommand> client
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
}