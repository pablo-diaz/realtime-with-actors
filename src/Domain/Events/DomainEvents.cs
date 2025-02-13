using Domain.Common;

namespace Domain.Events;

public abstract record DeviceEvent: IDomainEvent;

public sealed record DeviceHasBeenCreated(string DeviceId, Temperature WithTemperature, Coords AtLocation) : DeviceEvent;

public sealed record DeviceLocationHasChanged(string DeviceId, Coords PreviousLocation, Coords NewLocation) : DeviceEvent;

public sealed record DeviceTemperatureHasDecreased(string DeviceId, Coords WhenDeviceWasLocatedAt, Temperature PreviousTemperature, Temperature NewTemperature) : DeviceEvent;

public sealed record DeviceTemperatureHasIncreased(string DeviceId, Coords WhenDeviceWasLocatedAt, Temperature PreviousTemperature, Temperature NewTemperature) : DeviceEvent;

