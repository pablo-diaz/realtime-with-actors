using System;

using DeviceStateServices;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-metric")]
internal class InfluxDeviceMetric
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

    private InfluxDeviceMetric() {}

    public static InfluxDeviceMetric From(TemperatureMetric from) =>
        new InfluxDeviceMetric {
            DeviceId = from.DeviceId,
            Temperature = from.Temperature,
            Latitude = from.Location.Latitude,
            Longitude = from.Location.Longitude,
            LoggedAt = from.LoggedAt
        };
}
