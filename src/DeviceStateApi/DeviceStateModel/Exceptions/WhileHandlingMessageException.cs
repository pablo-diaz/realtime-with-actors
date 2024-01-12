namespace DeviceStateModel.Exceptions;

public class WhileHandlingMessageException: System.Exception
{
    public string Scenario { get; }

    public WhileHandlingMessageException(string scenario, string reason): base(message: reason)
    {
        Scenario = scenario;
    }
}