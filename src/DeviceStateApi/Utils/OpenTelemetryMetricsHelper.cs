using System;
using System.Diagnostics.Metrics;

using OpenTelemetry.Metrics;

namespace DeviceStateApi.Utils;

public static class OpenTelemetryMetricsHelper
{
    private const string MeterName = "DeviceStateModel";
    private const string MetricViewNameForCurrentLengthOfInboxQueueForAnActor = "device_state_actor_mailbox_length";
    private const int MaxNumberOfMetricEntriesAboutInboxLengthsOfActors = 20000;

    private static Meter Meter = new(name: MeterName, version: "1.0.0");

    private static readonly Gauge<long> CurrentActorMailboxLength = Meter.CreateGauge<long>(
        name: MetricViewNameForCurrentLengthOfInboxQueueForAnActor,
        description: "Current length of Inbox Queue for an Actor"
    );

    public sealed record CurrentLengthOfInboxQueueForAnActor(string Id, long CurrentLengthOfInboxQueue, Type ActorType);

    public static MeterProviderBuilder AddDeviceStateInstrumentation(this MeterProviderBuilder builder) =>
        builder.AddMeter(MeterName)
        .AddView(
            instrumentName: MetricViewNameForCurrentLengthOfInboxQueueForAnActor,
            metricStreamConfiguration: new MetricStreamConfiguration {
                CardinalityLimit = MaxNumberOfMetricEntriesAboutInboxLengthsOfActors
            }
        );

    public static void ReportMetric(CurrentLengthOfInboxQueueForAnActor ofCurrentLength)
    {
        CurrentActorMailboxLength.Record(
            value: ofCurrentLength.CurrentLengthOfInboxQueue,
            tags: [
                new("id", ofCurrentLength.Id),
                new("actortype", ofCurrentLength.ActorType.Name) ]);
    }

}
