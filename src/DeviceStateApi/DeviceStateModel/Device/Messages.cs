using System;

using Proto.Context;

namespace DeviceStateModel.Device;

public sealed record TemperatureTraced(string DeviceId, string LoggedAt, decimal Temperature, (decimal latitude, decimal longitude) Coords);

public sealed record NoRecentActivityHasBeenTrackedFromDevice(string DeviceId);

public sealed record WatchInboxLengthRequest(ActorContext Context, string Id, Type ActorType);
