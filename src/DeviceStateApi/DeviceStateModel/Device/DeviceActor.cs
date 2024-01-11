using System.Threading.Tasks;

using Proto;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly string _deviceId;
    private readonly string _initialLoggedDate;  // TODO: improve naming
    private decimal _currentTemperature;
    private Coords _currentCoords;

    public DeviceActor(string withDeviceId, string initialLoggedDate, decimal initialTemperature, Coords initialCoords)
    {
        this._deviceId = withDeviceId;
        this._initialLoggedDate = initialLoggedDate;
        this._currentTemperature = initialTemperature;
        this._currentCoords = initialCoords;

        System.Console.WriteLine($"[Device '{_deviceId}']: Created");
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        System.Console.WriteLine($"[Device '{_deviceId}']: New 'TemperatureTraced' event with Temp: '{message.Temperature}'");
        return Task.CompletedTask;
    }
}