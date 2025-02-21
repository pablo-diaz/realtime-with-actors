namespace DeviceStateModel.WatchingZone;

public record DeviceLocationChanged(string deviceId, string when, Domain.Coords toCoords);