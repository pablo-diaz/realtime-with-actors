using System;
using System.Threading.Tasks;

namespace DeviceStateServices;

public interface IEventStore: IDisposable
{
    Task StoreTemperatureMetric(TemperatureMetric @event);
    Task StoreDeviceCreatedEvent(DeviceCreatedEvent @event);
    Task StoreTemperatureChangeEvent(TemperatureChangeEvent @event);
    Task StoreLocationChangeEvent(LocationChangeEvent @event);
}

public record TemperatureMetric(string DeviceId, DateTimeOffset LoggedAt, decimal Temperature, (decimal Latitude, decimal Longitude) Location);

public record DeviceCreatedEvent(string DeviceId, DateTimeOffset LoggedAt, decimal withTemperature, (decimal Latitude, decimal Longitude) atLocation);

public record TemperatureChangeEvent(string DeviceId, DateTimeOffset LoggedAt, decimal PreviousTemperature, decimal NewTemperature, bool IsTemperatureIncrease, (decimal Latitude, decimal Longitude) Location);

public record LocationChangeEvent(string DeviceId, DateTimeOffset LoggedAt, (decimal Latitude, decimal Longitude) PreviousLocation, (decimal Latitude, decimal Longitude) NewLocation, decimal distanceInKms);
