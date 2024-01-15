using System.Threading.Tasks;

namespace DeviceStateServices;

public interface IUserEventPublisher
{
    Task PublishDeviceTemperatureHasIncreasedEvent(string forDeviceId, decimal previousTemperature, decimal newTemperature, (decimal latitude, decimal longitude) whileLocatedAt);
    Task PublishDeviceTemperatureHasDecreasedEvent(string forDeviceId, decimal previousTemperature, decimal newTemperature, (decimal latitude, decimal longitude) whileLocatedAt);
    Task PublishDeviceLocationHasChangedEvent(string forDeviceId, (decimal latitude, decimal longitude) previousLocation, (decimal latitude, decimal longitude) newLocation);
}