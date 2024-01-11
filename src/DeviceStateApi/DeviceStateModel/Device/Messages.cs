namespace DeviceStateModel.Device;

public record Coords(decimal Latitude, decimal Longitude);

public record TemperatureTraced(string DeviceId, string LoggedAt, decimal Temperature, Coords Coords);