using System.Linq;

using Domain;
using Domain.Events;

using NUnit.Framework;
using FluentAssertions;
using System.Collections.Generic;

namespace UnitTests;

public class DeviceTests
{
    [Test]
    public void Test_WhenCreating_IfValidDeviceIdIsProvided_ItWorks()
    {
        var newDeviceResult = Device.Create(deviceId: "AL-0001", initialTemperature: CreateTemperature(), initialCoords: CreateLocation());

        newDeviceResult.Should().NotBeNull();

        if (newDeviceResult.IsFailure)
            System.Console.WriteLine(newDeviceResult.Error);

        newDeviceResult.IsSuccess.Should().BeTrue();

        newDeviceResult.Value.Id.Should().Be("AL-0001");
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
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.3569M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.3569M), withSimilarityThreshold: withMultipleSimilarityThresholds);
        
        device.CurrentTemperature.Value.Should().Be(10.3569M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewHigherTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 1.0M);
        
        device.CurrentTemperature.Value.Should().Be(10.35M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewLowerTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.5M), withSimilarityThreshold: 1.0M);
        
        device.CurrentTemperature.Value.Should().Be(9.5M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfOnlyOneNewHigherTemperatureIsAboveSimilarityThreshold_NewerTemperatureShouldBeTracked_AndItShouldFireTemperatureIncreasedEvent()
    {
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 0.04M);
        
        device.CurrentTemperature.Value.Should().Be(10.35M);

        device.DomainEvents.Count().Should().Be(1);
        device.DomainEvents.First().Should().BeOfType<DeviceTemperatureHasIncreased>();
        
        var temperatureHasIncreasedEvent = (DeviceTemperatureHasIncreased)device.DomainEvents.First();
        temperatureHasIncreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasIncreasedEvent.NewTemperature.Value.Should().Be(10.35M);
        temperatureHasIncreasedEvent.PreviousTemperature.Value.Should().Be(10.30M);
    }

    [Test]
    public void Test_WhenChangingTemperature_IfVaringMultipleTemperatures_AndOnlyOneIsAboveSimilarityThreshold_NewestTemperatureShouldBeTracked_AndItShouldFireTemperatureIncreasedEvent()
    {
        var similatiryThreshold = 1.0M;
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.40M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.35M), withSimilarityThreshold: similatiryThreshold);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.25M), withSimilarityThreshold: similatiryThreshold);
        
        device.CurrentTemperature.Value.Should().Be(9.25M);

        device.DomainEvents.Count().Should().Be(1);
        device.DomainEvents.First().Should().BeOfType<DeviceTemperatureHasDecreased>();
        
        var temperatureHasDecreasedEvent = (DeviceTemperatureHasDecreased)device.DomainEvents.First();
        temperatureHasDecreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasDecreasedEvent.NewTemperature.Value.Should().Be(9.25M);
        temperatureHasDecreasedEvent.PreviousTemperature.Value.Should().Be(10.30M);
    }

    [Test]
    public void Test_WhenChangingTemperature_IfVaringMultipleTemperatures_AndTwoAreAboveSimilarityThreshold_NewestTemperatureShouldBeTracked_AndItShouldFireTwoTemperatureEvents()
    {
        var similatiryThreshold = 1.0M;
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 11.35M), withSimilarityThreshold: similatiryThreshold);
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 11.10M), withSimilarityThreshold: similatiryThreshold);

        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.05M), withSimilarityThreshold: similatiryThreshold);
        
        device.CurrentTemperature.Value.Should().Be(10.05M);

        device.DomainEvents.Count().Should().Be(2);
        
        device.DomainEvents.Any(e => e is DeviceTemperatureHasIncreased).Should().BeTrue();
        var temperatureHasIncreasedEvent = (DeviceTemperatureHasIncreased)device.DomainEvents.First(e => e is DeviceTemperatureHasIncreased);
        temperatureHasIncreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasIncreasedEvent.PreviousTemperature.Value.Should().Be(10.30M);
        temperatureHasIncreasedEvent.NewTemperature.Value.Should().Be(11.35M);

        device.DomainEvents.Any(e => e is DeviceTemperatureHasDecreased).Should().BeTrue();
        var temperatureHasDecreasedEvent = (DeviceTemperatureHasDecreased)device.DomainEvents.First(e => e is DeviceTemperatureHasDecreased);
        temperatureHasDecreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasDecreasedEvent.PreviousTemperature.Value.Should().Be(11.35M);
        temperatureHasDecreasedEvent.NewTemperature.Value.Should().Be(10.05M);
    }

    [Test]
    public void Test_WhenChangingTemperature_IfOnlyOneNewLowerTemperatureIsAboveSimilarityThreshold_NewerTemperatureShouldBeTracked_AndItShouldFireTemperatureDecreasedEvent()
    {
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.6M), withSimilarityThreshold: 0.05M);
        
        device.CurrentTemperature.Value.Should().Be(9.6M);

        device.DomainEvents.Count().Should().Be(1);
        device.DomainEvents.First().Should().BeOfType<DeviceTemperatureHasDecreased>();
        
        var temperatureHasDecreasedEvent = (DeviceTemperatureHasDecreased)device.DomainEvents.First();
        temperatureHasDecreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasDecreasedEvent.NewTemperature.Value.Should().Be(9.6M);
        temperatureHasDecreasedEvent.PreviousTemperature.Value.Should().Be(10.30M);
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsEqual_ItShouldNotFireAnyEvent([Values(0.0, 0.003, 1.0, 1.5)] decimal withMultipleToleratedDistancesInKm)
    {
        var device = CreateDevice(maybeWithLocation: CreateLocation(maybeWithLatitude: 15.35M, maybeWithLongitude: 123.0056M));
        
        device.ChangeLocation(newLocation: CreateLocation(maybeWithLatitude: 15.35M, maybeWithLongitude: 123.0056M), withAtLeastDistanceInKm: withMultipleToleratedDistancesInKm);
        
        device.CurrentLocation.Latitude.Should().Be(15.35M);
        device.CurrentLocation.Longitude.Should().Be(123.0056M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsCloserThanMinToleratedDistance_NewLocationShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M));
        
        device.ChangeLocation(newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 210.0M);
        
        device.CurrentLocation.Latitude.Should().Be(21.30M);
        device.CurrentLocation.Longitude.Should().Be(-75.1503M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingLocation_IfNewLocationIsFartherThanMinToleratedDistance_NewLocationShouldBeTracked_AndItShouldFireLocationChangedEvent()
    {
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithLocation: CreateLocation(maybeWithLatitude: 20.66M, maybeWithLongitude: -77.0366M));
        
        device.ChangeLocation(newLocation: CreateLocation(maybeWithLatitude: 21.30M, maybeWithLongitude: -75.1503M), withAtLeastDistanceInKm: 200.0M);
        
        device.CurrentLocation.Latitude.Should().Be(21.30M);
        device.CurrentLocation.Longitude.Should().Be(-75.1503M);

        device.DomainEvents.Count().Should().Be(1);
        device.DomainEvents.First().Should().BeOfType<DeviceLocationHasChanged>();
        
        var @event = (DeviceLocationHasChanged)device.DomainEvents.First();
        @event.DeviceId.Should().Be("AX-530");
        @event.NewLocation.Latitude.Should().Be(21.30M);
        @event.NewLocation.Longitude.Should().Be(-75.1503M);
        @event.PreviousLocation.Latitude.Should().Be(20.66M);
        @event.PreviousLocation.Longitude.Should().Be(-77.0366M);
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
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithLocation: loc1);
        
        device.ChangeLocation(newLocation: loc2, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc3, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc4, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc5, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc6, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        device.ChangeLocation(newLocation: loc7, withAtLeastDistanceInKm: minMovedToleratedDistanceInKm);
        
        device.CurrentLocation.Latitude.Should().Be(21.61M);
        device.CurrentLocation.Longitude.Should().Be(-77.19M);

        if(device.DomainEvents.Count() != 3)
        {
            foreach(var @event in device.DomainEvents)
            {
                if(@event is not DeviceLocationHasChanged locationChangedEvent)
                {
                    System.Console.WriteLine($"UnExpected event type found: {@event.GetType()}");
                    continue;
                }

                System.Console.WriteLine($"[{@event.GetType()}]: ({locationChangedEvent.PreviousLocation.Latitude}, {locationChangedEvent.PreviousLocation.Longitude}) -> ({locationChangedEvent.NewLocation.Latitude}, {locationChangedEvent.NewLocation.Longitude}) = {locationChangedEvent.PreviousLocation.GetDistanceInKm(to: locationChangedEvent.NewLocation)} Kms");
            }
        }

        device.DomainEvents.Should().HaveCount(3)
                           .And.AllBeAssignableTo<DeviceLocationHasChanged>();

        var firstEvent = device.DomainEvents[0] as DeviceLocationHasChanged;
        firstEvent.DeviceId.Should().Be("AX-530");
        firstEvent.PreviousLocation.Should().Be(loc1);
        firstEvent.NewLocation.Should().Be(loc3);

        var secondEvent = device.DomainEvents[1] as DeviceLocationHasChanged;
        secondEvent.DeviceId.Should().Be("AX-530");
        secondEvent.PreviousLocation.Should().Be(loc3);
        secondEvent.NewLocation.Should().Be(loc5);

        var thirdEvent = device.DomainEvents[2] as DeviceLocationHasChanged;
        thirdEvent.DeviceId.Should().Be("AX-530");
        thirdEvent.PreviousLocation.Should().Be(loc5);
        thirdEvent.NewLocation.Should().Be(loc6);
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
        ).Value;


    #endregion
}