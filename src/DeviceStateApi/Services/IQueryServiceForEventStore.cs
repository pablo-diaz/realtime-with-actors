using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DeviceStateApi.Services;

public interface IQueryServiceForEventStore: IDisposable
{
    Task<IReadOnlyList<Domain.Events.DeviceEvent>> GetEvents(string forDeviceId);
}
