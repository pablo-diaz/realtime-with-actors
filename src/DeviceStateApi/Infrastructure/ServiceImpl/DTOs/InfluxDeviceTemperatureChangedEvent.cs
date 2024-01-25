using System;

using DeviceStateServices;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-temperature-changed-event")]
internal class InfluxDeviceTemperatureChangedEvent
{
    [Column("device-id", IsTag = true)]
    public string? DeviceId { get; set; }

    [Column("previous-temperature")]
    public decimal? PreviousTemperature { get; set; }

    [Column("new-temperature")]
    public decimal? NewTemperature { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceTemperatureChangedEvent() {}

    public static InfluxDeviceTemperatureChangedEvent From(TemperatureChangeEvent from) =>
        new InfluxDeviceTemperatureChangedEvent {
            DeviceId = from.DeviceId,
            PreviousTemperature = from.PreviousTemperature,
            NewTemperature = from.NewTemperature,
            Latitude = from.Location.Latitude,
            Longitude = from.Location.Longitude,
            LoggedAt = from.LoggedAt
        };
}
