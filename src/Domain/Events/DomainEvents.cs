using Domain.Common;

namespace Domain.Events;

public abstract record DeviceEvent(string DeviceId) : IDomainEvent;

public sealed record DeviceHasBeenCreated(string DeviceId, Temperature WithTemperature, Coords AtLocation) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceLocationHasChanged(string DeviceId, Coords NewLocation) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceLocationHasChangedToAVeryCloseLocation(string DeviceId, Coords NewLocation) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceTemperatureHasDecreased(string DeviceId, Temperature NewTemperature) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceTemperatureHasIncreased(string DeviceId, Temperature NewTemperature) : DeviceEvent(DeviceId: DeviceId);

public sealed record SimilarDeviceTemperatureWasTraced(string DeviceId, Temperature NewTemperature) : DeviceEvent(DeviceId: DeviceId);
