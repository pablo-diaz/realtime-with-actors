using Domain.Common;

namespace Domain.Events
{
    public class DeviceHasBeenCreated: IDomainEvent
    {
        public string DeviceId { get; }
        public Temperature WithTemperature { get; }
        public Coords AtLocation { get; }

        public DeviceHasBeenCreated(string deviceId, Temperature withTemperature, Coords atLocation)
        {
            DeviceId = deviceId;
            WithTemperature = withTemperature;
            AtLocation = atLocation;
        }
    }
}