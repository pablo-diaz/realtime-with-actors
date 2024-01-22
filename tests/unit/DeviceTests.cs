using System.Linq;

using Domain;
using Domain.Events;

using NUnit.Framework;
using FluentAssertions;

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
        
        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.3569M), withSimilarityThreshold: withMultipleSimilarityThresholds);
        
        result.IsSuccess.Should().BeTrue();
        device.CurrentTemperature.Value.Should().Be(10.3569M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewHigherTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 1.0M);
        
        result.IsSuccess.Should().BeTrue();
        device.CurrentTemperature.Value.Should().Be(10.35M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfNewLowerTemperatureIsBelowSimilarityThreshold_NewTemperatureShouldBeTracked_ButItShouldNotFireAnyEvent()
    {
        var device = CreateDevice(maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.5M), withSimilarityThreshold: 1.0M);
        
        result.IsSuccess.Should().BeTrue();
        device.CurrentTemperature.Value.Should().Be(9.5M);
        device.DomainEvents.Any().Should().BeFalse();
    }

    [Test]
    public void Test_WhenChangingTemperature_IfOnlyOneNewHigherTemperatureIsAboveSimilarityThreshold_NewerTemperatureShouldBeTracked_AndItShouldFireTemperatureIncreasedEvent()
    {
        var device = CreateDevice(maybeWithDeviceId: "AX-530", maybeWithTemperature: CreateTemperature(maybeWithValue: 10.30M));
        
        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.35M), withSimilarityThreshold: 0.04M);
        
        result.IsSuccess.Should().BeTrue();
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

        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.25M), withSimilarityThreshold: similatiryThreshold);
        
        result.IsSuccess.Should().BeTrue();
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

        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 10.05M), withSimilarityThreshold: similatiryThreshold);
        
        result.IsSuccess.Should().BeTrue();
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
        
        var result = device.ChangeTemperature(newTemperature: CreateTemperature(maybeWithValue: 9.6M), withSimilarityThreshold: 0.05M);
        
        result.IsSuccess.Should().BeTrue();
        device.CurrentTemperature.Value.Should().Be(9.6M);

        device.DomainEvents.Count().Should().Be(1);
        device.DomainEvents.First().Should().BeOfType<DeviceTemperatureHasDecreased>();
        
        var temperatureHasDecreasedEvent = (DeviceTemperatureHasDecreased)device.DomainEvents.First();
        temperatureHasDecreasedEvent.DeviceId.Should().Be("AX-530");
        temperatureHasDecreasedEvent.NewTemperature.Value.Should().Be(9.6M);
        temperatureHasDecreasedEvent.PreviousTemperature.Value.Should().Be(10.30M);
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