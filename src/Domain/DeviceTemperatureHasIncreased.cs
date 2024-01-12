using Domain.Common;

namespace Domain.Events
{
    public class DeviceTemperatureHasIncreased: IDomainEvent
    {
        public string DeviceId { get; }
        public Coords WhenDeviceWasLocatedAt { get; }
        public decimal PreviousTemperature { get; }
        public decimal NewTemperature { get; }

        public DeviceTemperatureHasIncreased(string deviceId, Coords whenDeviceWasLocatedAt, decimal previousTemperature, decimal newTemperature)
        {
            DeviceId = deviceId;
            WhenDeviceWasLocatedAt = whenDeviceWasLocatedAt;
            PreviousTemperature = previousTemperature;
            NewTemperature = newTemperature;
        }
    }
}