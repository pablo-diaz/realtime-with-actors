using DeviceStateServices;

using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using Task = System.Threading.Tasks.Task;

namespace Infrastructure;

public sealed class InlfuxDbEventStore : IEventStore
{
    [Measurement("device-event")]
    private class InfluxDeviceEvent
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

        public static InfluxDeviceEvent From(TemperatureEvent from) =>
            new InfluxDeviceEvent {
                DeviceId = from.DeviceId,
                Temperature = from.Temperature,
                Latitude = from.Location.Latitude,
                Longitude = from.Location.Longitude,
                LoggedAt = from.LoggedAt
            };
    }

    private readonly InfluxDbConfig _configuration;

    public InlfuxDbEventStore(InfluxDbConfig config)
    {
        this._configuration = config;
    }

    public Task StoreTemperatureEvent(TemperatureEvent @event)
    {
        using var client = new InfluxDBClient(_configuration.ServiceUrl, _configuration?.ServiceToken);
        using var writeApi = client.GetWriteApi();
        writeApi.WriteMeasurement(InfluxDeviceEvent.From(@event), WritePrecision.Ns, _configuration?.Bucket, _configuration?.Organization);

        return Task.CompletedTask;
    }
}
