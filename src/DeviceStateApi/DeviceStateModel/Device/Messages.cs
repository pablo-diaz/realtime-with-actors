namespace DeviceStateModel.Device;

public record TemperatureTraced(string DeviceId, string LoggedAt, decimal Temperature, (decimal latitude, decimal longitude) Coords);