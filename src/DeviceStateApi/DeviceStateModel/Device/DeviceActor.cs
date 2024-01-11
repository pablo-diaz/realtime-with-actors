using System.Threading.Tasks;

using DeviceStateModel.Common;
using DeviceStateModel.WatchingZone;

using Proto;

namespace DeviceStateModel.Device;

public class DeviceActor: IActor
{
    private readonly string _deviceId;
    private readonly string _initialLoggedDate;  // TODO: improve naming
    private decimal _currentTemperature;
    private Coords _currentCoords;
    private readonly PID _watchingZoneManager;

    public DeviceActor(string withDeviceId, string initialLoggedDate, decimal initialTemperature, Coords initialCoords, PID watchingZoneManager)
    {
        this._deviceId = withDeviceId;
        this._initialLoggedDate = initialLoggedDate;
        this._currentTemperature = initialTemperature;
        this._currentCoords = initialCoords;
        this._watchingZoneManager = watchingZoneManager;
    }

    public Task ReceiveAsync(IContext context) => context.Message switch {
        TemperatureTraced temperatureMessage => Process(context, temperatureMessage),
        _ => Task.CompletedTask
    };

    private Task Process(IContext context, TemperatureTraced message)
    {
        ProcessTemperatureEvent(newTemperature: message.Temperature);
        ProcessCoordinatesEvent(context, when: message.LoggedAt, newCoords: message.Coords);
        return Task.CompletedTask;
    }

    private void ProcessTemperatureEvent(decimal newTemperature)
    {
        if(_currentTemperature == newTemperature)
            return;

        _currentTemperature = newTemperature;
        Log($"Temperature has changed to {newTemperature}");
    }

    private void ProcessCoordinatesEvent(IContext context, string when, Coords newCoords)
    {
        if(_currentCoords == newCoords)
            return;

        context.Send(_watchingZoneManager, new DeviceLocationChanged(deviceId: _deviceId, when: when,
            fromCoords: _currentCoords, toCoords: newCoords));

        _currentCoords = newCoords;

        Log($"Device location has changed to Lat: {newCoords.Latitude} - Lon: {newCoords.Longitude}");
    }

    private void Log(string message)
    {
        System.Console.WriteLine($"[Device {_deviceId}]: {message}");
    }
}