using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeviceStateApi.Services;

public interface IQueryServiceForEventStore
{
    Task<IReadOnlyList<Domain.Events.DeviceEvent>> GetEvents(string forDeviceId);
}
