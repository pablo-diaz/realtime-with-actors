using System.Threading.Tasks;

using Proto;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly string _deviceId;
    private readonly string initialLoggedDate;  // TODO: improve naming
    private readonly decimal _currentTemperature;
    private readonly Coords _currentCoords;

    public DeviceActor(string withDeviceId, string initialLoggedDate, decimal initialTemperature, Coords initialCoords)
    {
        this._deviceId = withDeviceId;
        this.initialLoggedDate = initialLoggedDate;
        this._currentTemperature = initialTemperature;
        this._currentCoords = initialCoords;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message) => Task.CompletedTask;
}