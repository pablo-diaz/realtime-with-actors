using System;
using System.Threading.Tasks;

namespace DeviceStateServices;

public interface IEventStore: IDisposable
{
    Task StoreEvent(Domain.Events.DeviceEvent @event, DateTimeOffset at);
}
