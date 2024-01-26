using Domain;

using NUnit.Framework;
using FluentAssertions;

namespace UnitTests;

public class CoordsTests
{
    [Test]
    public void Test_WhenCreating_IfValidCoordsAreProvided_ItWorks()
    {
        var newLocationResult = Coords.For(latitude: 10.35M, longitude: -40.65M);

        newLocationResult.Should().NotBeNull();

        if (newLocationResult.IsFailure)
            System.Console.WriteLine(newLocationResult.Error);

        newLocationResult.IsSuccess.Should().BeTrue();

        newLocationResult.Value.Latitude.Should().Be(10.35M);
        newLocationResult.Value.Longitude.Should().Be(-40.65M);
    }

    [Test]
    public void Test_WhenCreating_IfWrongLatitudeIsProvided_ItFails(
        [Values(-90.00001, 90.01)] decimal withLatitude)
    {
        var newLocationResult = Coords.For(latitude: withLatitude, longitude: -40.65M);
        newLocationResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Test_WhenCreating_IfWrongLongitudeIsProvided_ItFails(
        [Values(-180.00001, 180.01)] decimal withLongitude)
    {
        var newLocationResult = Coords.For(latitude: 85.6669M, longitude: withLongitude);
        newLocationResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Test_WhenGettingDistanceBetweenLocations()
    {
        var loc1 = Coords.For(latitude: 85.6669M, longitude: 130.5666M).Value;
        var loc2 = Coords.For(latitude: 85.6677M, longitude: 130.5688M).Value;

        var kms = loc1.GetDistanceInKm(to: loc2);

        kms.Should().Be(0.09M);
    }
}
