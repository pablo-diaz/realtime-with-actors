using System;

using Domain.Events;

using InfluxDB.Client.Core;

namespace Infrastructure.ServiceImpl.Dtos;

[Measurement("device-location-changed-event")]
internal class InfluxDeviceLocationChangedEvent
{
    [Column("device-id")]
    public string DeviceId { get; set; }

    [Column("new-latitude", IsTag = true)]
    public decimal NewLatitude { get; set; }

    [Column("new-longitude", IsTag = true)]
    public decimal NewLongitude { get; set; }

    [Column("distance-to-prev-loc-in-kms", IsTag = true)]
    public decimal DistanceInKms { get; set; }

    [Column(IsTimestamp = true)]
    public DateTimeOffset LoggedAt { get; set; }

    private InfluxDeviceLocationChangedEvent() {}

    public static InfluxDeviceLocationChangedEvent From(DeviceLocationHasChanged from, DateTimeOffset at) =>
        new InfluxDeviceLocationChangedEvent {
            DeviceId = from.DeviceId,
            NewLatitude = from.NewLocation.Latitude,
            NewLongitude = from.NewLocation.Longitude,
            DistanceInKms = from.PreviousLocation.GetDistanceInKm(to: from.NewLocation),
            LoggedAt = at
        };

}
