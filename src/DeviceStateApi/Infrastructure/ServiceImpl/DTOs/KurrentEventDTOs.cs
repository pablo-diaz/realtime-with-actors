using Domain;
using Domain.Events;

namespace DeviceStateApi.Infrastructure.ServiceImpl.DTOs;

internal sealed record KurrentDtoForDeviceHasBeenCreated(string DeviceId, decimal WithTemperature, decimal AtLatitude, decimal AtLongitude);
internal sealed record KurrentDtoForDeviceTemperatureHasIncreased(string DeviceId, decimal NewTemperature);
internal sealed record KurrentDtoForDeviceTemperatureHasDecreased(string DeviceId, decimal NewTemperature);
internal sealed record KurrentDtoForSimilarDeviceTemperatureWasTraced(string DeviceId, decimal NewTemperature);
internal sealed record KurrentDtoForDeviceLocationHasChanged(string DeviceId, decimal NewLatitude, decimal NewLongitude);
internal sealed record KurrentDtoForDeviceLocationHasChangedToAVeryCloseLocation(string DeviceId, decimal NewLatitude, decimal NewLongitude);

internal static class KurrentDtoSerializingUtils
{
    public static string SerializeToEventStore(DeviceHasBeenCreated e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForDeviceHasBeenCreated(DeviceId: e.DeviceId, WithTemperature: e.WithTemperature.Value, AtLatitude: e.AtLocation.Latitude, AtLongitude: e.AtLocation.Longitude));

    public static string SerializeToEventStore(DeviceTemperatureHasIncreased e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForDeviceTemperatureHasIncreased(DeviceId: e.DeviceId, NewTemperature: e.NewTemperature.Value));

    public static string SerializeToEventStore(DeviceTemperatureHasDecreased e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForDeviceTemperatureHasDecreased(DeviceId: e.DeviceId, NewTemperature: e.NewTemperature.Value));

    public static string SerializeToEventStore(SimilarDeviceTemperatureWasTraced e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForSimilarDeviceTemperatureWasTraced(DeviceId: e.DeviceId, NewTemperature: e.NewTemperature.Value));

    public static string SerializeToEventStore(DeviceLocationHasChanged e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForDeviceLocationHasChanged(DeviceId: e.DeviceId, NewLatitude: e.NewLocation.Latitude, NewLongitude: e.NewLocation.Longitude));

    public static string SerializeToEventStore(DeviceLocationHasChangedToAVeryCloseLocation e) =>
        System.Text.Json.JsonSerializer.Serialize(
            new KurrentDtoForDeviceLocationHasChangedToAVeryCloseLocation(DeviceId: e.DeviceId, NewLatitude: e.NewLocation.Latitude, NewLongitude: e.NewLocation.Longitude));

    public static DeviceHasBeenCreated DeserializeFromEventStore(KurrentDtoForDeviceHasBeenCreated dto) =>
            new DeviceHasBeenCreated(DeviceId: dto.DeviceId, WithTemperature: (Temperature)dto.WithTemperature, AtLocation: (Coords)(Latitude: dto.AtLatitude, Longitude: dto.AtLongitude));

    public static DeviceTemperatureHasIncreased DeserializeFromEventStore(KurrentDtoForDeviceTemperatureHasIncreased dto) =>
        new DeviceTemperatureHasIncreased(DeviceId: dto.DeviceId, NewTemperature: (Temperature)dto.NewTemperature);

    public static DeviceTemperatureHasDecreased DeserializeFromEventStore(KurrentDtoForDeviceTemperatureHasDecreased dto) =>
        new DeviceTemperatureHasDecreased(DeviceId: dto.DeviceId, (Temperature)dto.NewTemperature);

    public static SimilarDeviceTemperatureWasTraced DeserializeFromEventStore(KurrentDtoForSimilarDeviceTemperatureWasTraced dto) =>
        new SimilarDeviceTemperatureWasTraced(DeviceId: dto.DeviceId, (Temperature)dto.NewTemperature);

    public static DeviceLocationHasChanged DeserializeFromEventStore(KurrentDtoForDeviceLocationHasChanged dto) =>
        new DeviceLocationHasChanged(DeviceId: dto.DeviceId, NewLocation: (Coords)(Latitude: dto.NewLatitude, Longitude: dto.NewLongitude));

    public static DeviceLocationHasChangedToAVeryCloseLocation DeserializeFromEventStore(KurrentDtoForDeviceLocationHasChangedToAVeryCloseLocation dto) =>
        new DeviceLocationHasChangedToAVeryCloseLocation(DeviceId: dto.DeviceId, NewLocation: (Coords)(Latitude: dto.NewLatitude, Longitude: dto.NewLongitude));
}



