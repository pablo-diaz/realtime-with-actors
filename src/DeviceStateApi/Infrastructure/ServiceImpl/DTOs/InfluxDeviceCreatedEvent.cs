using System;

using Domain.Events;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-created-event")]
internal class InfluxDeviceCreatedEvent
{
    [Column("device-id")]
    public string DeviceId { get; set; }

    [Column("temperature", IsTag = true)]
    public decimal Temperature { get; set; }

    [Column("latitude", IsTag = true)]
    public decimal Latitude { get; set; }

    [Column("longitude", IsTag = true)]
    public decimal Longitude { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceCreatedEvent() {}

    public static InfluxDeviceCreatedEvent From(DeviceHasBeenCreated from, DateTimeOffset at) =>
        new InfluxDeviceCreatedEvent {
            DeviceId = from.DeviceId,
            Temperature = from.WithTemperature.Value,
            Latitude = from.AtLocation.Latitude,
            Longitude = from.AtLocation.Longitude,
            LoggedAt = at
        };
}
