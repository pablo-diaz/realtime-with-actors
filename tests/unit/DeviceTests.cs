using System;
using System.Linq;
using System.Collections.Generic;

using Domain;
using Domain.Common;
using Domain.Events;

using NUnit.Framework;
using FluentAssertions;

namespace UnitTests;

public class DeviceTests
{
    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToCreateNewDevice_IfExpectedParamsAreProvided_CommandOutcomeShouldSucceed()
    {
        var location = CreateLocation();
        var temperature = CreateTemperature();
        var resultOfCommandToCreateNewDevice = Device.Create(deviceId: "Unit-Testing-DeviceId-0001", initialTemperature: temperature, initialCoords: location);

        resultOfCommandToCreateNewDevice.Should().NotBeNull();

        if (resultOfCommandToCreateNewDevice.IsFailure)
            Console.WriteLine(resultOfCommandToCreateNewDevice.Error);

        resultOfCommandToCreateNewDevice.IsSuccess.Should().BeTrue();

        resultOfCommandToCreateNewDevice.Value.AggregateResult.Id.Should().Be("Unit-Testing-DeviceId-0001");
        resultOfCommandToCreateNewDevice.Value.AggregateResult.CurrentLocation.Should().Be(location);
        resultOfCommandToCreateNewDevice.Value.AggregateResult.CurrentTemperature.Should().Be(temperature);

        resultOfCommandToCreateNewDevice.Value.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new DeviceHasBeenCreated(DeviceId: "Unit-Testing-DeviceId-0001", WithTemperature: temperature, AtLocation: location));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForDeviceHasBeenCreated_ANewDeviceShouldBeCreatedSuccessfully()
    {
        var location = CreateLocation();
        var temperature = CreateTemperature();
        var eventToApply = new DeviceHasBeenCreated(DeviceId: "A device id to test with", WithTemperature: temperature, AtLocation: location);

        var newDevice = Device.Apply(@event: eventToApply, to: null);

        newDevice.Should().NotBeNull();

        newDevice.Id.Should().Be("A device id to test with");
        newDevice.CurrentLocation.Should().Be(location);
        newDevice.CurrentTemperature.Should().Be(temperature);
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToCreateNewDevice_IfWrongDeviceIdIsProvided_CommandShouldFail([Values(null, "")] string withDeviceId)
    {
        var resultOfCommandToCreateNewDevice = Device.Create(deviceId: withDeviceId, initialTemperature: CreateTemperature(), initialCoords: CreateLocation());

        resultOfCommandToCreateNewDevice.Should().NotBeNull();
        resultOfCommandToCreateNewDevice.IsFailure.Should().BeTrue();
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfNewTemperatureIsEqual_ItShouldNotFireAnyEvent([Values(0.0, 0.003, 1.0, 1.5)] decimal withMultipleSimilarityThresholds)
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.3569M));
        var commandOutcome = Device.ChangeTemperature(to: device, newTemperature: CreateTemperature(maybeWithValue: 10.3569M), withSimilarityThreshold: withMultipleSimilarityThresholds);
        
        device.CurrentTemperature.Should().Be((Temperature)10.3569M);
        commandOutcome.DomainEventsRaised.Should().BeEmpty();
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfNewHigherTemperatureIsBelowSimilarityThreshold_ItShouldRaiseEventForSimilarDeviceTemperatureWasTraced()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));

        var commandOutcome = Device.ChangeTemperature(to: device, newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 1.0M);
        
        device.CurrentTemperature.Should().Be((Temperature)10.30M);
        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new SimilarDeviceTemperatureWasTraced(DeviceId: device.Id, WhenDeviceWasLocatedAt: device.CurrentLocation,
                PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)10.35M));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForSimilarDeviceTemperatureWasTraced_NewTemperatureShouldHaveBeenApplied()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        var eventToApply = new SimilarDeviceTemperatureWasTraced(DeviceId: device.Id, WhenDeviceWasLocatedAt: device.CurrentLocation, PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)10.35M);

        device = Device.Apply(@event: eventToApply, to: device);

        device.Should().NotBeNull();

        device.CurrentTemperature.Should().Be((Temperature)10.35M);
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfNewLowerTemperatureIsBelowSimilarityThreshold_ItShouldRaiseEventForSimilarDeviceTemperatureWasTraced()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));

        var commandOutcome = Device.ChangeTemperature(to: device, newTemperature: CreateTemperature(maybeWithValue: 9.5M), withSimilarityThreshold: 1.0M);

        device.CurrentTemperature.Should().Be((Temperature)10.30M);
        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new SimilarDeviceTemperatureWasTraced(DeviceId: device.Id, WhenDeviceWasLocatedAt: device.CurrentLocation,
                PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)9.5M));
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfNewHigherTemperatureIsAboveSimilarityThreshold_ItShouldRaiseEventForDeviceTemperatureHasIncreased()
    {
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location);

        var commandOutcome = Device.ChangeTemperature(to: device, newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 0.04M);
        
        device.CurrentTemperature.Should().Be((Temperature)10.30M);
        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new DeviceTemperatureHasIncreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)10.35M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForDeviceTemperatureHasIncreased_NewTemperatureShouldHaveBeenApplied()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        var eventToApply = new DeviceTemperatureHasIncreased(DeviceId: device.Id, WhenDeviceWasLocatedAt: device.CurrentLocation, PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)11.55M);

        device = Device.Apply(@event: eventToApply, to: device);

        device.Should().NotBeNull();

        device.CurrentTemperature.Should().Be((Temperature)11.55M);
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfNewLowerTemperatureIsAboveSimilarityThreshold_ItShouldRaiseEventForDeviceTemperatureHasDecreased()
    {
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location);

        var commandOutcome = Device.ChangeTemperature(to: device, newTemperature: CreateTemperature(maybeWithValue: 8.55M), withSimilarityThreshold: 0.04M);

        device.CurrentTemperature.Should().Be((Temperature)10.30M);
        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new DeviceTemperatureHasDecreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)8.55M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForDeviceTemperatureHasDecreased_NewTemperatureShouldHaveBeenApplied()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        var eventToApply = new DeviceTemperatureHasDecreased(DeviceId: device.Id, WhenDeviceWasLocatedAt: device.CurrentLocation, PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)8.55M);

        device = Device.Apply(@event: eventToApply, to: device);

        device.Should().NotBeNull();

        device.CurrentTemperature.Should().Be((Temperature)8.55M);
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeTemperature_IfVaringMultipleTemperatures_AndOnlyOneIsAboveSimilarityThreshold_ItShouldRaiseEventForDeviceTemperatureHasIncreased()
    {
        var initialTemp = (Temperature)10.30M;
        var temp1 = (Temperature)10.35M;
        var temp2 = (Temperature)9.40M;
        var temp3 = (Temperature)9.35M;
        var temp4 = (Temperature)9.25M;

        var similatiryThreshold = 1.0M;
        var location = CreateLocation();
        var currentDeviceState = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: initialTemp, maybeWithLocation: location);

        var eventsRaisedSoFar = new List<DeviceEvent>();
        
        var commandOutcome = Device.ChangeTemperature(to: currentDeviceState, newTemperature: temp1, withSimilarityThreshold: similatiryThreshold);
        currentDeviceState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: currentDeviceState);
        eventsRaisedSoFar.AddRange(commandOutcome.DomainEventsRaised.Cast<DeviceEvent>());

        commandOutcome = Device.ChangeTemperature(to: currentDeviceState, newTemperature: temp2, withSimilarityThreshold: similatiryThreshold);
        currentDeviceState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: currentDeviceState);
        eventsRaisedSoFar.AddRange(commandOutcome.DomainEventsRaised.Cast<DeviceEvent>());

        commandOutcome = Device.ChangeTemperature(to: currentDeviceState, newTemperature: temp3, withSimilarityThreshold: similatiryThreshold);
        currentDeviceState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: currentDeviceState);
        eventsRaisedSoFar.AddRange(commandOutcome.DomainEventsRaised.Cast<DeviceEvent>());

        commandOutcome = Device.ChangeTemperature(to: currentDeviceState, newTemperature: temp4, withSimilarityThreshold: similatiryThreshold);
        currentDeviceState = ApplyEvents(eventsToApply: commandOutcome.DomainEventsRaised.Cast<DeviceEvent>(), toInitialState: currentDeviceState);
        eventsRaisedSoFar.AddRange(commandOutcome.DomainEventsRaised.Cast<DeviceEvent>());

        eventsRaisedSoFar.Should()
            .HaveCount(4)
            .And.ContainInConsecutiveOrder(
                new SimilarDeviceTemperatureWasTraced(DeviceId: "AX-530", WhenDeviceWasLocatedAt: location, PreviousTemperature: initialTemp, NewTemperature: temp1),
                new SimilarDeviceTemperatureWasTraced(DeviceId: "AX-530", WhenDeviceWasLocatedAt: location, PreviousTemperature: temp1, NewTemperature: temp2),
                new SimilarDeviceTemperatureWasTraced(DeviceId: "AX-530", WhenDeviceWasLocatedAt: location, PreviousTemperature: temp2, NewTemperature: temp3),
                new DeviceTemperatureHasDecreased(DeviceId: "AX-530", WhenDeviceWasLocatedAt: location, PreviousTemperature: initialTemp, NewTemperature: temp4)
            );

        currentDeviceState.CurrentTemperature.Should().Be(temp4);
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeLocation_IfNewLocationIsEqualToPreviousOne_ItShouldNotFireAnyEvent([Values(0.0, 0.003, 1.0, 1.5)] decimal withMultipleToleratedDistancesInKm)
    {
        var device = CreateDevice(maybeWithLocation: (Coords)(Latitude: 15.35M, Longitude: 123.0056M));

        var commandOutcome = Device.ChangeLocation(to: device, newLocation: (Coords)(Latitude: 15.35M, Longitude: 123.0056M), withAtLeastDistanceInKm: withMultipleToleratedDistancesInKm);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 15.35M, Longitude: 123.0056M));
        commandOutcome.DomainEventsRaised.Should().BeEmpty();
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeLocation_IfNewLocationIsCloserThanMinToleratedDistance_ItShouldFireEventAboutDeviceLocationHasChangedToAVeryCloseLocation()
    {
        var device = CreateDevice(maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M));

        var commandOutcome = Device.ChangeLocation(to: device, newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 210.0M);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 20.66M, Longitude: -77.0366M));
        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new DeviceLocationHasChangedToAVeryCloseLocation(DeviceId: device.Id,
                PreviousLocation: (Coords)(Latitude: 20.66M, Longitude: -77.0366M), NewLocation: (Coords)(Latitude: 21.30M, Longitude: -75.1503M)));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForDeviceLocationHasChangedToAVeryCloseLocation_NewLocationShouldBeTracked()
    {
        var device = CreateDevice(maybeWithLocation: (Coords)(Latitude: 20.66M, Longitude: -77.0366M));
        var eventToApply = new DeviceLocationHasChangedToAVeryCloseLocation(DeviceId: device.Id,
                PreviousLocation: device.CurrentLocation, NewLocation: (Coords)(Latitude: 21.30M, Longitude: -75.1503M));

        device = Device.Apply(@event: eventToApply, to: device);

        device.Should().NotBeNull();

        device.CurrentLocation.Should().Be((Coords)(Latitude: 21.30M, Longitude: -75.1503M));
    }

    [Test]
    [Category(name: "Issuing Commands")]
    public void Test_WhenCommandIsIssuedToChangeLocation_IfNewLocationIsFartherThanMinToleratedDistance_ItShouldFireLocationChangedEvent()
    {
        var device = CreateDevice(maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M));

        var commandOutcome = Device.ChangeLocation(to: device, newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 200.0M);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 20.66M, Longitude: -77.0366M));

        commandOutcome.DomainEventsRaised.Should()
            .HaveCount(1)
            .And.ContainEquivalentOf(new DeviceLocationHasChanged(
                DeviceId: device.Id,
                PreviousLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M),
                NewLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M)));
    }

    [Test]
    [Category(name: "Applying Events")]
    public void Test_WhenApplyingEventForDeviceLocationHasChanged_NewLocationShouldBeTracked()
    {
        var device = CreateDevice(maybeWithLocation: (Coords)(Latitude: 20.66M, Longitude: -77.0366M));
        var eventToApply = new DeviceLocationHasChanged(DeviceId: device.Id,
                PreviousLocation: device.CurrentLocation, NewLocation: (Coords)(Latitude: 21.30M, Longitude: -75.1503M));

        device = Device.Apply(@event: eventToApply, to: device);

        device.Should().NotBeNull();

        device.CurrentLocation.Should().Be((Coords)(Latitude: 21.30M, Longitude: -75.1503M));
    }

    #region Helpers

    private static Temperature CreateTemperature(decimal? maybeWithValue = 15.5M) =>
        Temperature.For(value: maybeWithValue.Value).Value;

    private static Coords CreateLocation(decimal? maybeWithLatitude = 15.56M, decimal? maybeWithLongitude = 45.56M) =>
        Coords.For(latitude: maybeWithLatitude.Value, longitude: maybeWithLongitude.Value).Value;

    private static Device CreateDevice(string maybeWithDeviceId = "Al-0001", Temperature maybeWithTemperature = null, Coords maybeWithLocation = null) =>
        Device.Create(deviceId: maybeWithDeviceId,
            initialTemperature: maybeWithTemperature != null ? maybeWithTemperature : CreateTemperature(),
            initialCoords: maybeWithLocation != null ? maybeWithLocation : CreateLocation()
        ).Value.AggregateResult;

    private static Domain.Device ApplyEvents(IEnumerable<DeviceEvent> eventsToApply, Domain.Device toInitialState) =>
        eventsToApply.Aggregate(seed: toInitialState,
            func: (mostRecentState, eventToApply) => Domain.Device.Apply(@event: eventToApply, to: mostRecentState));


    #endregion

}