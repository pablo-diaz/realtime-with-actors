using System;

using DeviceStateServices;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-location-changed-event")]
internal class InfluxDeviceLocationChangedEvent
{
    [Column("device-id", IsTag = true)]
    public string? DeviceId { get; set; }

    [Column("previous-latitude")]
    public decimal? PreviousLatitude { get; set; }

    [Column("previous-longitude")]
    public decimal? PreviousLongitude { get; set; }

    [Column("new-latitude")]
    public decimal? NewLatitude { get; set; }

    [Column("new-longitude")]
    public decimal? NewLongitude { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceLocationChangedEvent() {}

    public static InfluxDeviceLocationChangedEvent From(LocationChangeEvent from) =>
        new InfluxDeviceLocationChangedEvent {
            DeviceId = from.DeviceId,
            PreviousLatitude = from.PreviousLocation.Latitude,
            PreviousLongitude = from.PreviousLocation.Longitude,
            NewLatitude = from.NewLocation.Latitude,
            NewLongitude = from.NewLocation.Longitude,
            LoggedAt = from.LoggedAt
        };
}
