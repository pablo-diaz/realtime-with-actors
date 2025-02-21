namespace DeviceStateApi.Infrastructure.ServiceImpl;

internal static class FluxQueries
{
    public static string GetAllEventsForDevice(string forDeviceId) => @"

        queryParams = {
          deviceToQuery: """ + forDeviceId + @"""
        }

        temperatures = from(bucket: ""device-events-db"")
          |> range(start: 0)
          |> filter(fn: (r) => r._measurement == ""device-temperature-changed-event"")
          |> filter(fn: (r) => r._field == ""device-id"")
          |> filter(fn: (r) => r._value == queryParams.deviceToQuery)
          |> keep(columns: [""_measurement"", ""_time"", ""event-type"", ""new-temperature""])

        creations = from(bucket: ""device-events-db"")
          |> range(start: 0)
          |> filter(fn: (r) => r[""_measurement""] == ""device-created-event"")
          |> filter(fn: (r) => r[""_field""] == ""device-id"")
          |> filter(fn: (r) => r._value == queryParams.deviceToQuery)
          |> keep(columns: [""_measurement"", ""_time"", ""latitude"", ""longitude"", ""temperature""])

        locations = from(bucket: ""device-events-db"")
          |> range(start: 0)
          |> filter(fn: (r) => r[""_measurement""] == ""device-location-changed-event"")
          |> filter(fn: (r) => r[""_field""] == ""device-id"")
          |> filter(fn: (r) => r._value == queryParams.deviceToQuery)
          |> keep(columns: [""_measurement"", ""_time"", ""event-type"", ""new-latitude"", ""new-longitude""])

        union(tables: [temperatures, creations, locations])
          |> group()
          |> sort(columns: [""_time""])

    ";

}
