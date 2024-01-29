using Task = System.Threading.Tasks.Task;

using DeviceStateServices;

using Infrastructure.ServiceImpl.Dtos;

using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace Infrastructure;

public sealed class InlfuxDbEventStore : IEventStore
{
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

    public Task StoreTemperatureMetric(TemperatureMetric @event)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceMetric.From(@event), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    public Task StoreDeviceCreatedEvent(DeviceCreatedEvent @event)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceCreatedEvent.From(@event), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    public Task StoreTemperatureChangeEvent(TemperatureChangeEvent @event)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceTemperatureChangedEvent.From(@event), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    public Task StoreLocationChangeEvent(LocationChangeEvent @event)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceLocationChangedEvent.From(@event), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }
}
