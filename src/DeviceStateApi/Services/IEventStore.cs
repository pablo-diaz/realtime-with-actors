using System;
using System.Threading.Tasks;

namespace DeviceStateServices;

public interface IEventStore
{
    Task StoreTemperatureEvent(TemperatureEvent @event);
}

public record TemperatureEvent(string DeviceId, DateTimeOffset LoggedAt, decimal Temperature, (decimal Latitude, decimal Longitude) Location);
