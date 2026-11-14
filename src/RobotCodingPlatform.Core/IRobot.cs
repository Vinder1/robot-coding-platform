namespace RobotCodingPlatform.Core;

public interface IRobot
{
    void ExecuteCommand(string name, IReadOnlyList<int> arguments);
    bool GetSensor(string name);
}