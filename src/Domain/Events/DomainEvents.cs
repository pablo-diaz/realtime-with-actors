using Domain.Common;

namespace Domain.Events;

public abstract record DeviceEvent(string DeviceId) : IDomainEvent;

public sealed record DeviceHasBeenCreated(string DeviceId, Temperature WithTemperature, Coords AtLocation) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceLocationHasChanged(string DeviceId, Coords PreviousLocation, Coords NewLocation) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceTemperatureHasDecreased(string DeviceId, Coords WhenDeviceWasLocatedAt, Temperature PreviousTemperature, Temperature NewTemperature) : DeviceEvent(DeviceId: DeviceId);

public sealed record DeviceTemperatureHasIncreased(string DeviceId, Coords WhenDeviceWasLocatedAt, Temperature PreviousTemperature, Temperature NewTemperature) : DeviceEvent(DeviceId: DeviceId);

