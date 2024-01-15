using Domain.Common;

namespace Domain.Events
{
    public class DeviceTemperatureHasIncreased: IDomainEvent
    {
        public string DeviceId { get; }
        public Coords WhenDeviceWasLocatedAt { get; }
        public Temperature PreviousTemperature { get; }
        public Temperature NewTemperature { get; }

        public DeviceTemperatureHasIncreased(string deviceId, Coords whenDeviceWasLocatedAt, Temperature previousTemperature, Temperature newTemperature)
        {
            DeviceId = deviceId;
            WhenDeviceWasLocatedAt = whenDeviceWasLocatedAt;
            PreviousTemperature = previousTemperature;
            NewTemperature = newTemperature;
        }
    }
}