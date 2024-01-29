using System;

using DeviceStateServices;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-created-event")]
internal class InfluxDeviceCreatedEvent
{
    [Column("device-id", IsTag = true)]
    public string? DeviceId { get; set; }

    [Column("temperature")]
    public decimal? Temperature { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceCreatedEvent() {}

    public static InfluxDeviceCreatedEvent From(DeviceCreatedEvent from) =>
        new InfluxDeviceCreatedEvent {
            DeviceId = from.DeviceId,
            Temperature = from.withTemperature,
            Latitude = from.atLocation.Latitude,
            Longitude = from.atLocation.Longitude,
            LoggedAt = from.LoggedAt
        };
}
