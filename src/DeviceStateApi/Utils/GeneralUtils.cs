using System;
using System.Linq;

using DeviceStateModel.Device;
using DeviceStateApi.DeviceStateModel.Device;

using Proto;
using Proto.Timers;
using Proto.Context;

namespace DeviceStateApi.Utils;

public static class GeneralUtils
{
    public static void PrintMessageToConsoleWithSpecialChar(string messageWithSpecialChar)
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.Write(messageWithSpecialChar);
    }

    public static void StartPeriodicTaskToReportMetricAboutCurrentUserMessageCount(
        IContext ofActor, string withId, Type ActorType, TimeSpan frequencyReportingThisMetric)
    {
        if (ofActor is not ActorContext) return;

        if (frequencyReportingThisMetric == TimeSpan.Zero) return;

        ofActor.Scheduler().SendRepeatedly(
            message: new WatchInboxLengthRequest(Context: ofActor as ActorContext, Id: withId, ActorType: ActorType),
            interval: frequencyReportingThisMetric, delay: TimeSpan.FromSeconds(1),
            target: ofActor.System.ProcessRegistry.Find(pattern: MetricsReporterActor.ActorNameInRegistry).First());
    }

}
