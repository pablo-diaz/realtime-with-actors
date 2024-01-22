using Domain;

using NUnit.Framework;
using FluentAssertions;

namespace UnitTests;

public class TemperatureTests
{
    [Test]
    public void Test_WhenCreating_IfValidTemperatureIsProvided_ItWorks(
        [Values(-100.0, -10.5, 0.0, 50.3, 100.0)] decimal withTemperature)
    {
        var newTemperatureResult = Temperature.For(value: withTemperature);

        newTemperatureResult.Should().NotBeNull();

        if (newTemperatureResult.IsFailure)
            System.Console.WriteLine(newTemperatureResult.Error);

        newTemperatureResult.IsSuccess.Should().BeTrue();

        newTemperatureResult.Value.Value.Should().Be(withTemperature);
    }

    [Test]
    public void Test_WhenCreating_IfWrongTemperatureIsProvided_ItFails(
        [Values(-100.1, 100.1)] decimal withTemperature)
    {
        var newTemperatureResult = Temperature.For(value: withTemperature);

        newTemperatureResult.Should().NotBeNull();
        newTemperatureResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Test_WhenComparingTemperatures_SmallerVsHigher()
    {
        var smallerTemperature = Temperature.For(value: 15.5556M).Value;
        var higherTemperature = Temperature.For(value: 15.5557M).Value;

        (smallerTemperature < higherTemperature).Should().BeTrue();
        (smallerTemperature == higherTemperature).Should().BeFalse();
        (smallerTemperature > higherTemperature).Should().BeFalse();
    }

    [Test]
    public void Test_WhenComparingTemperatures_Equal()
    {
        var oneTemperature = Temperature.For(value: 15.555555M).Value;
        var anotherTemperature = Temperature.For(value: 15.555555M).Value;

        (oneTemperature < anotherTemperature).Should().BeFalse();
        (oneTemperature == anotherTemperature).Should().BeTrue();
        (oneTemperature > anotherTemperature).Should().BeFalse();
    }
}
