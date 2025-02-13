using Domain.Common;
using Domain.Events;

using CSharpFunctionalExtensions;

namespace Domain;

public class Device : AggregateRoot<string>
{
    public Temperature CurrentTemperature { get; private set; }
    private Temperature _mostRecentTemperatureNotifiedFor;

    public Coords CurrentLocation { get; private set; }
    private Coords _mostRecentLocationNotifiedFor;

    private Device(string id, Temperature initialTemperature, Coords initialCoords): base(id)
    {
        CurrentTemperature = initialTemperature;
        _mostRecentTemperatureNotifiedFor = initialTemperature;

        CurrentLocation = initialCoords;
        _mostRecentLocationNotifiedFor = initialCoords;

        RaiseDomainEvent(new DeviceHasBeenCreated(DeviceId: id, WithTemperature: CurrentTemperature, AtLocation: CurrentLocation));
    }

    public static Result<Device> Create(string deviceId, Temperature initialTemperature, Coords initialCoords)
    {
        if(string.IsNullOrEmpty(deviceId))
            return Result.Failure<Device>("Please provide a valid Device Id");

        return new Device(id: deviceId, initialTemperature: initialTemperature, initialCoords: initialCoords);
    }

    public void ChangeTemperature(Temperature newTemperature, decimal withSimilarityThreshold)
    {
        if(CurrentTemperature == newTemperature)
            return;

        var previousMostRecentTemperatureNotifiedFor = _mostRecentTemperatureNotifiedFor;
        CurrentTemperature = newTemperature;

        if(newTemperature.IsSimilar(to: _mostRecentTemperatureNotifiedFor, belowSimilarityThreshold: withSimilarityThreshold))
            return;

        _mostRecentTemperatureNotifiedFor = newTemperature;

        if(newTemperature < previousMostRecentTemperatureNotifiedFor)
            RaiseDomainEvent(new DeviceTemperatureHasDecreased(DeviceId: Id, WhenDeviceWasLocatedAt: CurrentLocation,
                PreviousTemperature: previousMostRecentTemperatureNotifiedFor, NewTemperature: newTemperature));
        else 
            RaiseDomainEvent(new DeviceTemperatureHasIncreased(DeviceId: Id, WhenDeviceWasLocatedAt: CurrentLocation,
                PreviousTemperature: previousMostRecentTemperatureNotifiedFor, NewTemperature: newTemperature));
        
        return;
    }

    public void ChangeLocation(Coords newLocation, decimal withAtLeastDistanceInKm)
    {
        if(CurrentLocation == newLocation)
            return;

        var previousMostRecentLocationNotifiedFor = _mostRecentLocationNotifiedFor;
        CurrentLocation = newLocation;

        if(newLocation.GetDistanceInKm(to: _mostRecentLocationNotifiedFor) < withAtLeastDistanceInKm)
            return;

        _mostRecentLocationNotifiedFor = newLocation;

        RaiseDomainEvent(new DeviceLocationHasChanged(DeviceId: Id, PreviousLocation: previousMostRecentLocationNotifiedFor, NewLocation: newLocation));
    }
}
