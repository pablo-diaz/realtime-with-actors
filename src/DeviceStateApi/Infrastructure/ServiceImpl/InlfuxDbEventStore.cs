using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DeviceStateApi.Services;

using Domain.Events;

using Infrastructure.ServiceImpl.Dtos;

using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace Infrastructure.ServiceImpl;

public sealed class InlfuxDbEventStore : IEventStore
{
    private readonly InfluxDbConfig _configuration;
    private readonly InfluxDBClient _client;
    private readonly WriteApiAsync _asyncWritter;

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

    public Task StoreEvent(DeviceEvent @event, DateTimeOffset at) => @event switch {
        DeviceHasBeenCreated e =>                           StoreEvent(e, at),
        DeviceTemperatureHasIncreased e =>                  StoreEvent(e, at),
        DeviceTemperatureHasDecreased e =>                  StoreEvent(e, at),
        SimilarDeviceTemperatureWasTraced e =>              StoreEvent(e, at),
        DeviceLocationHasChanged e =>                       StoreEvent(e, at),
        DeviceLocationHasChangedToAVeryCloseLocation e =>   StoreEvent(e, at),
        _ =>                                                Task.CompletedTask
    };

    private Task StoreEvent(DeviceHasBeenCreated @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceCreatedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    private Task StoreEvent(DeviceTemperatureHasIncreased @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceTemperatureChangedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    private Task StoreEvent(DeviceTemperatureHasDecreased @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceTemperatureChangedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    private Task StoreEvent(SimilarDeviceTemperatureWasTraced @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceTemperatureChangedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    private Task StoreEvent(DeviceLocationHasChanged @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceLocationChangedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

    private Task StoreEvent(DeviceLocationHasChangedToAVeryCloseLocation @event, DateTimeOffset at)
    {
        return _asyncWritter?.WriteMeasurementAsync(measurement: InfluxDeviceLocationChangedEvent.From(@event, at), precision: WritePrecision.Ns,
            bucket: _configuration?.Bucket, org: _configuration?.Organization)!;
    }

}
