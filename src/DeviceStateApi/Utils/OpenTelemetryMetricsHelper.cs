using System;
using System.Diagnostics.Metrics;

using OpenTelemetry.Metrics;

namespace DeviceStateApi.Utils;

public static class OpenTelemetryMetricsHelper
{
    private const string MeterName = "DeviceStateModel";

    private const int MaxNumberOfMetricEntriesPerCategory = 20_000;

    private static Meter Meter = new(name: MeterName, version: "1.0.0");

    private const string MetricViewNameForCurrentLengthOfInboxQueueForAnActor = "device_state_actor_mailbox_length";

    private static readonly Gauge<long> CurrentActorMailboxLength = Meter.CreateGauge<long>(
        name: MetricViewNameForCurrentLengthOfInboxQueueForAnActor,
        description: "Current length of Inbox Queue for an Actor"
    );

    private static readonly double[] QueueLengthHistogramBoundaries =
        { 0, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };

    public sealed record CurrentLengthOfInboxQueueForAnActor(string Id, long CurrentLengthOfInboxQueue, Type ActorType);

    public static MeterProviderBuilder AddDeviceStateInstrumentation(this MeterProviderBuilder builder)
    {
        builder = builder.AddMeter(MeterName);

        builder.AddView(
            instrumentName: MetricViewNameForCurrentLengthOfInboxQueueForAnActor,
            metricStreamConfiguration: new ExplicitBucketHistogramConfiguration {
                Boundaries = QueueLengthHistogramBoundaries,  
                CardinalityLimit = MaxNumberOfMetricEntriesPerCategory  // https://github.com/open-telemetry/opentelemetry-dotnet/releases/tag/core-1.10.0-rc.1
            }
        );

        return builder;
    }

    public static void ReportMetric(CurrentLengthOfInboxQueueForAnActor ofCurrentLength)
    {
        CurrentActorMailboxLength.Record(
            value: ofCurrentLength.CurrentLengthOfInboxQueue,
            tags: [
                new("id", ofCurrentLength.Id),
                new("actortype", ofCurrentLength.ActorType.Name) ]);
    }

}
