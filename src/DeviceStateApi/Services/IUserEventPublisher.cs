using System.Threading.Tasks;

namespace DeviceStateServices;

public interface IUserEventPublisher
{
    Task PublishDeviceHasBeenCreatedEvent(string forDeviceId, decimal withTemperature, (decimal latitude, decimal longitude) atLocation);
    Task PublishDeviceTemperatureHasIncreasedEvent(string forDeviceId, decimal newTemperature);
    Task PublishDeviceTemperatureHasDecreasedEvent(string forDeviceId, decimal newTemperature);
    Task PublishDeviceLocationHasChangedEvent(string forDeviceId, (decimal latitude, decimal longitude) newLocation);
}