using System;
using Task = System.Threading.Tasks.Task;

using DeviceStateServices;

using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;

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

    private readonly InfluxDbConfig? _configuration;
    private readonly InfluxDBClient? _client;
    private readonly WriteApiAsync? _asyncWritter;

    public InlfuxDbEventStore(InfluxDbConfig config)
    {
        this._configuration = config;
        this._client = new InfluxDBClient(_configuration.ServiceUrl, _configuration?.ServiceToken);
        this._asyncWritter = this._client.GetWriteApiAsync();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public Task StoreTemperatureEvent(TemperatureEvent @event)
    {
        //System.Console.WriteLine($"[Influx Db Impl] Writing event for '{@event.DeviceId}' with Temp: {@event.Temperature}");
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceEvent.From(@event), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }
}
