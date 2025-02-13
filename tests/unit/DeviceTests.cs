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
    public void Test_WhenCreating_IfValidDeviceIdIsProvided_ItWorks()
    {
        var newDeviceResult = Device.Create(deviceId: "Unit-Testing-DeviceId-0001", initialTemperature: CreateTemperature(), initialCoords: CreateLocation());

        newDeviceResult.Should().NotBeNull();

        if (newDeviceResult.IsFailure)
            System.Console.WriteLine(newDeviceResult.Error);

        newDeviceResult.IsSuccess.Should().BeTrue();

        newDeviceResult.Value.Id.Should().Be("Unit-Testing-DeviceId-0001");
        newDeviceResult.Value.DomainEvents.Should().HaveCount(1).And.AllBeOfType<DeviceHasBeenCreated>();
    }

    [Test]
    public void Test_WhenCreating_IfWrongDeviceIdIsProvided_ItFails([Values(null, "")] string withDeviceId)
    {
        var newDeviceResult = Device.Create(deviceId: withDeviceId, initialTemperature: CreateTemperature(), initialCoords: CreateLocation());

        newDeviceResult.Should().NotBeNull();
        newDeviceResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewTemperatureIsEqual_ItShouldNotFireAnyEvent([Values(0.0, 0.003, 1.0, 1.5)] decimal withMultipleSimilarityThresholds)
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.3569M), removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.3569M), withSimilarityThreshold: withMultipleSimilarityThresholds);
        
        device.CurrentTemperature.Should().Be((Temperature)10.3569M);
        device.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewHigherTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 1.0M);
        
        device.CurrentTemperature.Should().Be((Temperature)10.35M);
        device.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewLowerTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.5M), withSimilarityThreshold: 1.0M);
        
        device.CurrentTemperature.Should().Be((Temperature)9.5M);
        device.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfOnlyOneNewHigherTemperatureIsAboveSimilarityThreshold_NewerTemperatureShouldBeTracked_AndItShouldFireTemperatureIncreasedEvent()
    {
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location, removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 0.04M);
        
        device.CurrentTemperature.Should().Be((Temperature)10.35M);

        device.DomainEvents.Should().HaveCount(1)
            .And.ContainEquivalentOf(new DeviceTemperatureHasIncreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)10.35M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    public void Test_WhenChangingTemperature_IfVaringMultipleTemperatures_AndOnlyOneIsAboveSimilarityThreshold_NewestTemperatureShouldBeTracked_AndItShouldFireTemperatureIncreasedEvent()
    {
        var similatiryThreshold = 1.0M;
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location, removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.40M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.35M), withSimilarityThreshold: similatiryThreshold);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.25M), withSimilarityThreshold: similatiryThreshold);
        
        device.CurrentTemperature.Should().Be((Temperature)9.25M);

        device.DomainEvents.Should().HaveCount(1)
            .And.ContainEquivalentOf(new DeviceTemperatureHasDecreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)9.25M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    public void Test_WhenChangingTemperature_IfVaringMultipleTemperatures_AndTwoAreAboveSimilarityThreshold_NewestTemperatureShouldBeTracked_AndItShouldFireTwoTemperatureEvents()
    {
        var similatiryThreshold = 1.0M;
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location, removeDeviceHasBeenCreatedEvent: true);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 11.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 11.10M), withSimilarityThreshold: similatiryThreshold);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.05M), withSimilarityThreshold: similatiryThreshold);
        
        device.CurrentTemperature.Should().Be((Temperature)10.05M);

        device.DomainEvents.Should()
            .HaveCount(2)
            .And.ContainEquivalentOf(new DeviceTemperatureHasIncreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)11.35M, WhenDeviceWasLocatedAt: location))
            .And.ContainEquivalentOf(new DeviceTemperatureHasDecreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)11.35M, NewTemperature: (Temperature)10.05M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    public void Test_WhenChangingTemperature_IfOnlyOneNewLowerTemperatureIsAboveSimilarityThreshold_NewerTemperatureShouldBeTracked_AndItShouldFireTemperatureDecreasedEvent()
    {
        var location = CreateLocation();
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M), maybeWithLocation: location, removeDeviceHasBeenCreatedEvent: true);
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.6M), withSimilarityThreshold: 0.05M);
        
        device.CurrentTemperature.Should().Be((Temperature)9.6M);

        device.DomainEvents.Should().HaveCount(1)
            .And.ContainEquivalentOf(new DeviceTemperatureHasDecreased(DeviceId: "AX-530", PreviousTemperature: (Temperature)10.30M, NewTemperature: (Temperature)9.6M, WhenDeviceWasLocatedAt: location));
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsEqual_ItShouldNotFireAnyEvent([Values(0.0, 0.003, 1.0, 1.5)] decimal withMultipleToleratedDistancesInKm)
    {
        var device = CreateDevice(maybeWithLocation: (Coords)(Latitude: 15.35M, Longitude: 123.0056M), removeDeviceHasBeenCreatedEvent: true);
        
        device.ChangeLocation(newLocation: (Coords)(Latitude: 15.35M, Longitude: 123.0056M), withAtLeastDistanceInKm: withMultipleToleratedDistancesInKm);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 15.35M, Longitude: 123.0056M));
        device.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsCloserThanMinToleratedDistance_NewLocationShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M), removeDeviceHasBeenCreatedEvent: true);
        
        device.ChangeLocation(newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 210.0M);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 21.30M, Longitude: -75.1503M));
        device.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsFartherThanMinToleratedDistance_NewLocationShouldBeTracked_AndItShouldFireLocationChangedEvent()
    {
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M), removeDeviceHasBeenCreatedEvent: true);
        
        device.ChangeLocation(newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 200.0M);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 21.30M, Longitude: -75.1503M));

        device.DomainEvents.Should().HaveCount(1)
            .And.ContainEquivalentOf(new DeviceLocationHasChanged(
                DeviceId: "AX-530",
                PreviousLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M),
                NewLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M)));
    }

    [Test]
    public void Test_WhenChangingLocation_IfMovingMultiplePlaces_AndThreeMovedDistancesAreFartherThanMinToleratedDistance_NewestMovedLocationShouldBeTracked_AndItShouldFireThreeLocationChangedEvents()
    {
        var loc1 = CreateLocation(maybeWithLatitude: 20.66M,  maybeWithLongitude: -77.0366M);
        var loc2 = CreateLocation(maybeWithLatitude: 20.66M,  maybeWithLongitude: -77.0377M);
        var loc3 = CreateLocation(maybeWithLatitude: 21.55M,  maybeWithLongitude: -77.0377M);
        var loc4 = CreateLocation(maybeWithLatitude: 21.566M, maybeWithLongitude: -77.13M  );
        var loc5 = CreateLocation(maybeWithLatitude: 21.40M,  maybeWithLongitude: -77.15M  );
        var loc6 = CreateLocation(maybeWithLatitude: 21.60M,  maybeWithLongitude: -77.20M  );
        var loc7 = CreateLocation(maybeWithLatitude: 21.61M,  maybeWithLongitude: -77.19M  );

        var minMovedToleratedDistanceInKm = 20.0M;
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithLocation: loc1, removeDeviceHasBeenCreatedEvent: true);
        
        device.ChangeLocation(newLocation: loc2, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc3, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc4, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc5, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc6, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc7, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        
        device.CurrentLocation.Should().Be((Coords)(Latitude: 21.61M, Longitude: -77.19M));

        if(device.DomainEvents.Count() != 3) // for tracing test purposes
            PrintError<DeviceLocationHasChanged>(forEvents: device.DomainEvents,
                printExpectedEventInfo: locationChangedEvent => Console.WriteLine($"[{locationChangedEvent.GetType()}]: ({locationChangedEvent.PreviousLocation.Latitude}, {locationChangedEvent.PreviousLocation.Longitude}) -> ({locationChangedEvent.NewLocation.Latitude}, {locationChangedEvent.NewLocation.Longitude}) = {locationChangedEvent.PreviousLocation.GetDistanceInKm(to: locationChangedEvent.NewLocation)} Kms"));

        device.DomainEvents.Should()
            .HaveCount(3)
            .And.ContainInConsecutiveOrder(
                new DeviceLocationHasChanged(DeviceId: "AX-530", PreviousLocation: loc1, NewLocation: loc3),
                new DeviceLocationHasChanged(DeviceId: "AX-530", PreviousLocation: loc3, NewLocation: loc5),
                new DeviceLocationHasChanged(DeviceId: "AX-530", PreviousLocation: loc5, NewLocation: loc6)
            );
    }

    #region Helpers

    private static Temperature CreateTemperature(decimal? maybeWithValue = 15.5M) =>
        Temperature.For(value: maybeWithValue.Value).Value;

    private static Coords CreateLocation(decimal? maybeWithLatitude = 15.56M, decimal? maybeWithLongitude = 45.56M) =>
        Coords.For(latitude: maybeWithLatitude.Value, longitude: maybeWithLongitude.Value).Value;

    private static Device CreateDevice(string maybeWithDeviceId = "Al-0001", Temperature maybeWithTemperature = null,
        Coords maybeWithLocation = null, bool removeDeviceHasBeenCreatedEvent = false)
    {
        var result = Device.Create(deviceId: maybeWithDeviceId,
            initialTemperature: maybeWithTemperature != null ? maybeWithTemperature : CreateTemperature(),
            initialCoords: maybeWithLocation != null ? maybeWithLocation : CreateLocation()
        ).Value;

        if (removeDeviceHasBeenCreatedEvent)
            result.ClearDomainEvents();

        return result;
    }

    private static void PrintError<TExpectedEventType>(IReadOnlyList<IDomainEvent> forEvents, Action<TExpectedEventType> printExpectedEventInfo) where TExpectedEventType : DeviceEvent
    {
        foreach (var @event in forEvents)
        {
            if (@event is not TExpectedEventType expectedEvent)
            {
                Console.WriteLine($"UnExpected event type found: {@event.GetType()}");
                continue;
            }

            printExpectedEventInfo(@event as TExpectedEventType);
        }
    }

    #endregion

}