using Domain.Common;

namespace Domain.Events
{
    public class DeviceLocationHasChanged: IDomainEvent
    {
        public string DeviceId { get; }
        public Coords PreviousLocation { get; }
        public Coords NewLocation { get; }

        public DeviceLocationHasChanged(string deviceId, Coords previousLocation, Coords newLocation)
        {
            DeviceId = deviceId;
            PreviousLocation = previousLocation;
            NewLocation = newLocation;
        }
    }
}