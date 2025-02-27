using System;

using Domain.Events;
using DeviceStateApi.Infrastructure.ServiceImpl.DTOs;

namespace DeviceStateApi.Infrastructure.ServiceImpl;

internal static class KurrentDbUtils
{
    public static string GetStreamName(string forDeviceId) => $"device-{forDeviceId}";

    public static string GetEventType(this DeviceEvent of) => of switch {
        DeviceHasBeenCreated e =>                           "device-has-been-created",
        DeviceTemperatureHasIncreased e =>                  "device-temperature-has-increased",
        DeviceTemperatureHasDecreased e =>                  "device-temperature-has-decreased",
        SimilarDeviceTemperatureWasTraced e =>              "similar-device-temperature-was-traced",
        DeviceLocationHasChanged e =>                       "device-location-has-changed",
        DeviceLocationHasChangedToAVeryCloseLocation e =>   "device-location-has-changed-to-a-very-close-location",
        _ => throw new ApplicationException($"UnExpected device event type '{of.GetType()}'")
    };

    public static string SerializeToEventStore(this DeviceEvent @event) => @event switch {
        DeviceHasBeenCreated e =>                           KurrentDtoSerializingUtils.SerializeToEventStore(e),
        DeviceTemperatureHasIncreased e =>                  KurrentDtoSerializingUtils.SerializeToEventStore(e),
        DeviceTemperatureHasDecreased e =>                  KurrentDtoSerializingUtils.SerializeToEventStore(e),
        SimilarDeviceTemperatureWasTraced e =>              KurrentDtoSerializingUtils.SerializeToEventStore(e),
        DeviceLocationHasChanged e =>                       KurrentDtoSerializingUtils.SerializeToEventStore(e),
        DeviceLocationHasChangedToAVeryCloseLocation e =>   KurrentDtoSerializingUtils.SerializeToEventStore(e),
        _ => throw new ApplicationException($"UnExpected device event type '{@event.GetType()}'")
    };

    public static DeviceEvent DeserializeFromEventStore(string serializedData, string fromEventType) => fromEventType switch {
        "device-has-been-created" =>
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForDeviceHasBeenCreated>(serializedData)),
        "device-temperature-has-increased" =>
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForDeviceTemperatureHasIncreased>(serializedData)),
        "device-temperature-has-decreased" =>
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForDeviceTemperatureHasDecreased>(serializedData)),
        "similar-device-temperature-was-traced" => 
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForSimilarDeviceTemperatureWasTraced>(serializedData)),
        "device-location-has-changed" => 
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForDeviceLocationHasChanged>(serializedData)),
        "device-location-has-changed-to-a-very-close-location" => 
            KurrentDtoSerializingUtils.DeserializeFromEventStore(
                System.Text.Json.JsonSerializer.Deserialize<KurrentDtoForDeviceLocationHasChangedToAVeryCloseLocation>(serializedData)),
        _ => throw new ApplicationException($"UnExpected device event type '{fromEventType}'")
    };

}
