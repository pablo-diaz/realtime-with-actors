using Domain.Common;

namespace Domain.Events
{
    public class DeviceTemperatureHasDecreased: IDomainEvent
    {
        public string DeviceId { get; }
        public Coords WhenDeviceWasLocatedAt { get; }
        public Temperature PreviousTemperature { get; }
        public Temperature NewTemperature { get; }

        public DeviceTemperatureHasDecreased(string deviceId, Coords whenDeviceWasLocatedAt, Temperature previousTemperature, Temperature newTemperature)
        {
            DeviceId = deviceId;
            WhenDeviceWasLocatedAt = whenDeviceWasLocatedAt;
            PreviousTemperature = previousTemperature;
            NewTemperature = newTemperature;
        }
    }
}