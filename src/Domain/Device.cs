using Domain.Common;
using Domain.Events;

using CSharpFunctionalExtensions;

namespace Domain
{
    public class Device : AggregateRoot<string>
    {
        public Temperature CurrentTemperature { get; private set; }
        public Coords CurrentLocation { get; private set; }

        private Device(string id, Temperature initialTemperature, Coords initialCoords): base(id)
        {
            CurrentTemperature = initialTemperature;
            CurrentLocation = initialCoords;
        }

        public static Result<Device> Create(string deviceId, Temperature initialTemperature, Coords initialCoords)
        {
            if(string.IsNullOrEmpty(deviceId))
                return Result.Failure<Device>("Please provide a valid Device Id");

            return new Device(id: deviceId, initialTemperature: initialTemperature, initialCoords: initialCoords);
        }

        public Result ChangeTemperature(Temperature newTemperature)
        {
            if(CurrentTemperature == newTemperature)
                return Result.Success();

            var previousTemperature = CurrentTemperature;
            CurrentTemperature = newTemperature;

            if(newTemperature < previousTemperature)
                RaiseDomainEvent(new DeviceTemperatureHasDecreased(deviceId: Id, whenDeviceWasLocatedAt: CurrentLocation,
                    previousTemperature: previousTemperature, newTemperature: newTemperature));
            else 
                RaiseDomainEvent(new DeviceTemperatureHasIncreased(deviceId: Id, whenDeviceWasLocatedAt: CurrentLocation,
                    previousTemperature: previousTemperature, newTemperature: newTemperature));
            
            return Result.Success();
        }

        public void ChangeLocation(Coords newLocation)
        {
            if(CurrentLocation == newLocation)
                return;

            var previousLocation = CurrentLocation;
            CurrentLocation = newLocation;

            RaiseDomainEvent(new DeviceLocationHasChanged(deviceId: Id, previousLocation: previousLocation, newLocation: newLocation));
        }
    }
}
