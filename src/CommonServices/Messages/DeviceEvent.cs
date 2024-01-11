namespace Messages
{
    public class DeviceEvent
    {
        // "yyyy-MM-dd HH:mm:ss"
        public string At { get; set; }
        public string DeviceId { get; set; }
        public decimal Temperature { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}