using System.Threading.Tasks;
using System.Collections.Generic;

using Domain;
using Domain.Events;

using Infrastructure;
using DeviceStateApi.Services;

using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;

namespace DeviceStateApi.Infrastructure.ServiceImpl;

public sealed class QueryServiceForEventStoreBasedOnInfluxDb: IQueryServiceForEventStore
{
    private readonly InfluxDbConfig _configuration;
    private readonly InfluxDBClient _client;

    public QueryServiceForEventStoreBasedOnInfluxDb(InfluxDbConfig config)
    {
        this._configuration = config;
        this._client = new InfluxDBClient(_configuration.ServiceUrl, _configuration?.ServiceToken);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public async Task<IReadOnlyList<DeviceEvent>> GetEvents(string forDeviceId)
    {
        // TODO: prevent "sql"-injection attacks by sanetizing 'forDeviceId' param (maybe by avoiding primitive obsession??, i.e. turning 'forDeviceId' into its own Value Object where these validations would fit)
        var eventsQueried = new List<DeviceEvent>();

        (await _client
            .GetQueryApi()
            .QueryAsync(org: _configuration.Organization, query: FluxQueries.GetAllEventsForDevice(forDeviceId))
        )
        .ForEach(fluxTable => eventsQueried.AddRange(GetEvents(forDeviceId, fromTable: fluxTable)));

        return eventsQueried;
    }

    private static IEnumerable<DeviceEvent> GetEvents(string forDeviceId, FluxTable fromTable)
    {
        foreach (var record in fromTable.Records)
            yield return Map(forDeviceId, fluxRecord: record);
    }

    private static DeviceEvent Map(string forDeviceId, FluxRecord fluxRecord)
    {
        var eventType = fluxRecord.GetMeasurement();
        return eventType switch {
            "device-created-event" =>               MapForCreationEvent(forDeviceId, fluxRecord),
            "device-temperature-changed-event" =>   MapForTemperatureEvent(forDeviceId, fluxRecord),
            "device-location-changed-event" =>      MapForLocationEvent(forDeviceId, fluxRecord),
            _ => throw new System.ApplicationException($"UnExpected event type (influx measurement) '{eventType}' ")
        };
    }

    private static DeviceEvent MapForCreationEvent(string forDeviceId, FluxRecord fluxRecord) =>
        new DeviceHasBeenCreated(
            DeviceId: forDeviceId,
            WithTemperature: (Temperature) GetTagValueAsDecimal(fluxRecord, tagName: "temperature"),
            AtLocation: (Coords)(
                Latitude: GetTagValueAsDecimal(fluxRecord, tagName: "latitude"),
                Longitude: GetTagValueAsDecimal(fluxRecord, tagName: "longitude")));

    private static DeviceEvent MapForTemperatureEvent(string forDeviceId, FluxRecord fluxRecord)
    {
        var temperatureType = GetTagValue(fluxRecord, tagName: "event-type");
        return temperatureType switch {
            
            "SimilarDeviceTemperatureWasTraced" =>
                new SimilarDeviceTemperatureWasTraced(
                    DeviceId: forDeviceId,
                    NewTemperature: (Temperature)GetTagValueAsDecimal(fluxRecord, tagName: "new-temperature")),

            "DeviceTemperatureHasIncreased" =>
                new DeviceTemperatureHasIncreased(
                    DeviceId: forDeviceId,
                    NewTemperature: (Temperature)GetTagValueAsDecimal(fluxRecord, tagName: "new-temperature")),

            "DeviceTemperatureHasDecreased" =>
                new DeviceTemperatureHasDecreased(
                    DeviceId: forDeviceId,
                    NewTemperature: (Temperature)GetTagValueAsDecimal(fluxRecord, tagName: "new-temperature")),

            _ => throw new System.ApplicationException($"UnExpected temperature type '{temperatureType}' ")
        };
    }

    private static DeviceEvent MapForLocationEvent(string forDeviceId, FluxRecord fluxRecord)
    {
        var locationType = GetTagValue(fluxRecord, tagName: "event-type");
        return locationType switch
        {

            "DeviceLocationHasChanged" =>
                new DeviceLocationHasChanged(
                    DeviceId: forDeviceId,
                    NewLocation: (Coords)(
                        Latitude: GetTagValueAsDecimal(fluxRecord, tagName: "new-latitude"),
                        Longitude: GetTagValueAsDecimal(fluxRecord, tagName: "new-longitude"))),

            "DeviceLocationHasChangedToAVeryCloseLocation" =>
                new DeviceLocationHasChangedToAVeryCloseLocation(
                    DeviceId: forDeviceId,
                    NewLocation: (Coords)(
                        Latitude: GetTagValueAsDecimal(fluxRecord, tagName: "new-latitude"),
                        Longitude: GetTagValueAsDecimal(fluxRecord, tagName: "new-longitude"))),

            _ => throw new System.ApplicationException($"UnExpected location type '{locationType}' ")
        };
    }

    private static decimal GetTagValueAsDecimal(FluxRecord fluxRecord, string tagName) =>
        decimal.Parse(fluxRecord.GetValueByKey(key: tagName).ToString());

    private static string GetTagValue(FluxRecord fluxRecord, string tagName) =>
        fluxRecord.GetValueByKey(key: tagName).ToString();

}
