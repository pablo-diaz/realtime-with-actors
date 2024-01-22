using Domain.Common;
using Domain.Events;

using CSharpFunctionalExtensions;

namespace Domain
{
    public class Device : AggregateRoot<string>
    {
        public Temperature CurrentTemperature { get; private set; }
        private Temperature _mostRecentTemperatureNotifiedFor;
        public Coords CurrentLocation { get; private set; }

        private Device(string id, Temperature initialTemperature, Coords initialCoords): base(id)
        {
            CurrentTemperature = initialTemperature;
            _mostRecentTemperatureNotifiedFor = initialTemperature;
            CurrentLocation = initialCoords;
        }

        public static Result<Device> Create(string deviceId, Temperature initialTemperature, Coords initialCoords)
        {
            if(string.IsNullOrEmpty(deviceId))
                return Result.Failure<Device>("Please provide a valid Device Id");

            return new Device(id: deviceId, initialTemperature: initialTemperature, initialCoords: initialCoords);
        }

        public Result ChangeTemperature(Temperature newTemperature, decimal withSimilarityThreshold)
        {
            if(CurrentTemperature == newTemperature)
                return Result.Success();

            var previousMostRecentTemperatureNotifiedFor = _mostRecentTemperatureNotifiedFor;
            CurrentTemperature = newTemperature;

            if(newTemperature.IsSimilar(to: _mostRecentTemperatureNotifiedFor, belowSimilarityThreshold: withSimilarityThreshold))
                return Result.Success();

            _mostRecentTemperatureNotifiedFor = newTemperature;

            if(newTemperature < previousMostRecentTemperatureNotifiedFor)
                RaiseDomainEvent(new DeviceTemperatureHasDecreased(deviceId: Id, whenDeviceWasLocatedAt: CurrentLocation,
                    previousTemperature: previousMostRecentTemperatureNotifiedFor, newTemperature: newTemperature));
            else 
                RaiseDomainEvent(new DeviceTemperatureHasIncreased(deviceId: Id, whenDeviceWasLocatedAt: CurrentLocation,
                    previousTemperature: previousMostRecentTemperatureNotifiedFor, newTemperature: newTemperature));
            
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
