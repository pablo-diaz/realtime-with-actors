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
    }

    #region Commands

    public static Result<CommandOutcomeWithAggregate<Device>> Create(string deviceId, Temperature initialTemperature, Coords initialCoords)
    {
        if(string.IsNullOrEmpty(deviceId))
            return Result.Failure<CommandOutcomeWithAggregate<Device>>("Please provide a valid Device Id");

        var newDevice = new Device(id: deviceId, initialTemperature: initialTemperature, initialCoords:initialCoords);

        return new CommandOutcomeWithAggregate<Device>(AggregateResult: newDevice,
            DomainEventsRaised: [new DeviceHasBeenCreated(DeviceId: newDevice.Id, WithTemperature: newDevice.CurrentTemperature, AtLocation: newDevice.CurrentLocation)]);
    }

    public static CommandOutcome ChangeTemperature(Device to, Temperature newTemperature, decimal withSimilarityThreshold)
    {
        if(to.CurrentTemperature == newTemperature)
            return CommandOutcome.WithNoEvents();

        if(newTemperature.IsSimilar(to: to._mostRecentTemperatureNotifiedFor, belowSimilarityThreshold: withSimilarityThreshold))
            return new CommandOutcome(DomainEventsRaised: [
                new SimilarDeviceTemperatureWasTraced(DeviceId: to.Id, WhenDeviceWasLocatedAt: to.CurrentLocation,
                PreviousTemperature: to.CurrentTemperature, NewTemperature: newTemperature)]);

        var previousMostRecentTemperatureNotifiedFor = to._mostRecentTemperatureNotifiedFor;

        if(newTemperature < previousMostRecentTemperatureNotifiedFor)
            return new CommandOutcome(DomainEventsRaised: [
                new DeviceTemperatureHasDecreased(DeviceId: to.Id, WhenDeviceWasLocatedAt: to.CurrentLocation, 
                PreviousTemperature: previousMostRecentTemperatureNotifiedFor, NewTemperature: newTemperature)]);
        else
            return new CommandOutcome(DomainEventsRaised: [
                new DeviceTemperatureHasIncreased(DeviceId: to.Id, WhenDeviceWasLocatedAt: to.CurrentLocation,
                PreviousTemperature: previousMostRecentTemperatureNotifiedFor, NewTemperature: newTemperature)]);
    }

    public static CommandOutcome ChangeLocation(Device to, Coords newLocation, decimal withAtLeastDistanceInKm)
    {
        if(to.CurrentLocation == newLocation)
            return CommandOutcome.WithNoEvents();

        if (newLocation.GetDistanceInKm(to: to._mostRecentLocationNotifiedFor) < withAtLeastDistanceInKm)
            return new CommandOutcome(DomainEventsRaised: [
                new DeviceLocationHasChangedToAVeryCloseLocation(DeviceId: to.Id, PreviousLocation: to.CurrentLocation, NewLocation: newLocation)]);

        var previousMostRecentLocationNotifiedFor = to._mostRecentLocationNotifiedFor;

        return new CommandOutcome(DomainEventsRaised: [
            new DeviceLocationHasChanged(DeviceId: to.Id, PreviousLocation: previousMostRecentLocationNotifiedFor, NewLocation: newLocation)]);
    }

    #endregion

    #region Event appliers

    public static Device Apply(DeviceEvent @event, Device to) => @event switch {
        DeviceHasBeenCreated e when to is null =>                               Apply(e),
        SimilarDeviceTemperatureWasTraced e when to is not null =>              to.Apply(e),
        DeviceTemperatureHasDecreased e when to is not null =>                  to.Apply(e),
        DeviceTemperatureHasIncreased e when to is not null =>                  to.Apply(e),
        DeviceLocationHasChangedToAVeryCloseLocation e when to is not null =>   to.Apply(e),
        DeviceLocationHasChanged e when to is not null =>                       to.Apply(e),
        _ =>                                                                    to
    };

    private static Device Apply(DeviceHasBeenCreated @event) =>
        new Device(id: @event.DeviceId, initialTemperature: @event.WithTemperature, initialCoords: @event.AtLocation);

    private Device Apply(SimilarDeviceTemperatureWasTraced @event)
    {
        if (@event.DeviceId != this.Id) return this;

        CurrentTemperature = @event.NewTemperature;
        return this;
    }

    private Device Apply(DeviceTemperatureHasDecreased @event)
    {
        if (@event.DeviceId != this.Id) return this;

        CurrentTemperature = @event.NewTemperature;
        _mostRecentTemperatureNotifiedFor = @event.NewTemperature;
        return this;
    }

    private Device Apply(DeviceTemperatureHasIncreased @event)
    {
        if (@event.DeviceId != this.Id) return this;

        CurrentTemperature = @event.NewTemperature;
        _mostRecentTemperatureNotifiedFor = @event.NewTemperature;
        return this;
    }

    private Device Apply(DeviceLocationHasChangedToAVeryCloseLocation @event)
    {
        if (@event.DeviceId != this.Id) return this;

        CurrentLocation = @event.NewLocation;
        return this;
    }

    private Device Apply(DeviceLocationHasChanged @event)
    {
        if (@event.DeviceId != this.Id) return this;

        CurrentLocation = @event.NewLocation;
        _mostRecentLocationNotifiedFor = @event.NewLocation;
        return this;
    }

    #endregion

}
