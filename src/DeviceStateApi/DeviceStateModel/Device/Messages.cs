using DeviceStateModel.Common;

namespace DeviceStateModel.Device;

public record TemperatureTraced(string DeviceId, string LoggedAt, decimal Temperature, Coords Coords);