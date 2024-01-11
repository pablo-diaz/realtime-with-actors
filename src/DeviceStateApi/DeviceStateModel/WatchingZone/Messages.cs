using DeviceStateModel.Common;

namespace DeviceStateModel.WatchingZone;

public record DeviceLocationChanged(string deviceId, string when, Coords fromCoords, Coords toCoords);