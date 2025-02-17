namespace Messages;

// 'At' should be in the following date format: "yyyy-MM-dd HH:mm:ss"
public sealed record DeviceEvent(string At, string DeviceId, decimal Temperature, decimal Latitude, decimal Longitude);
