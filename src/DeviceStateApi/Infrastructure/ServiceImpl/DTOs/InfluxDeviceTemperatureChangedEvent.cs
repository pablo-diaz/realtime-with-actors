using System;

using Domain.Events;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-temperature-changed-event")]
internal class InfluxDeviceTemperatureChangedEvent
{
    [Column("device-id")]
    public string DeviceId { get; set; }

    [Column("new-temperature", IsTag = true)]
    public decimal NewTemperature { get; set; }

    [Column("latitude", IsTag = true)]
    public decimal Latitude { get; set; }

    [Column("longitude", IsTag = true)]
    public decimal Longitude { get; set; }

    [Column("event-type", IsTag = true)]
    public string EventType { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceTemperatureChangedEvent() {}

    public static InfluxDeviceTemperatureChangedEvent From(DeviceTemperatureHasIncreased from, DateTimeOffset at) =>
        new InfluxDeviceTemperatureChangedEvent {
            DeviceId = from.DeviceId,
            NewTemperature = from.NewTemperature.Value,
            Latitude = from.WhenDeviceWasLocatedAt.Latitude,
            Longitude = from.WhenDeviceWasLocatedAt.Longitude,
            LoggedAt = at,
            EventType = "DeviceTemperatureHasIncreased"
        };

    public static InfluxDeviceTemperatureChangedEvent From(DeviceTemperatureHasDecreased from, DateTimeOffset at) =>
        new InfluxDeviceTemperatureChangedEvent {
            DeviceId = from.DeviceId,
            NewTemperature = from.NewTemperature.Value,
            Latitude = from.WhenDeviceWasLocatedAt.Latitude,
            Longitude = from.WhenDeviceWasLocatedAt.Longitude,
            LoggedAt = at,
            EventType = "DeviceTemperatureHasDecreased"
        };

    public static InfluxDeviceTemperatureChangedEvent From(SimilarDeviceTemperatureWasTraced from, DateTimeOffset at) =>
        new InfluxDeviceTemperatureChangedEvent
        {
            DeviceId = from.DeviceId,
            NewTemperature = from.NewTemperature.Value,
            Latitude = from.WhenDeviceWasLocatedAt.Latitude,
            Longitude = from.WhenDeviceWasLocatedAt.Longitude,
            LoggedAt = at,
            EventType = "SimilarDeviceTemperatureWasTraced"
        };

}
