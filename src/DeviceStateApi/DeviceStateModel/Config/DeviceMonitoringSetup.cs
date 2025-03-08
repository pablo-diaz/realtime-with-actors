namespace DeviceStateModel.Config;

public class DeviceMonitoringSetup
{
    public decimal TemperatureSimilarityThreshold { get; set; }
    public decimal MinMovedToleratedDistanceInKms { get; set; }
    public int MinsToShutDownIdleDevice { get; set; }
    public bool ShouldTryToLoadActorStateFromEventLogStream { get; set; }
    public int FrequencyInSecondsOfReportingMetricAboutInboxLengthOfActors { get; set; }
}