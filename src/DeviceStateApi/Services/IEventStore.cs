using System;
using System.Threading.Tasks;

namespace DeviceStateApi.Services;

public interface IEventStore: IDisposable
{
    Task StoreEvent(Domain.Events.DeviceEvent @event, DateTimeOffset at);
}
