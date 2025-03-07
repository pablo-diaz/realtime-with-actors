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

    public static void StartPeriodicTaskToReportMetricAboutCurrentUserMessageCount(IContext ofActor, string withId, Type ActorType)
    {
        if (ofActor is not ActorContext) return;

        ofActor.Scheduler().SendRepeatedly(
            message: new WatchInboxLengthRequest(Context: ofActor as ActorContext, Id: withId, ActorType: ActorType),
            interval: TimeSpan.FromSeconds(5), delay: TimeSpan.FromSeconds(1),
            target: ofActor.System.ProcessRegistry.Find(pattern: MetricsReporterActor.ActorNameInRegistry).First());

        /*
        Task.Run(async () => await
            RunPeriodicActionRepeatedly(
                action: ReportMetricAboutCurrentUserMessageCount,
                actionParam: new ActorInfo(Context: ofActor as ActorContext, DeviceId: withDeviceId, ActorType: typeof(DeviceActor)),
                interval: TimeSpan.FromSeconds(5)));
        */
    }

    /*
    private static void ReportMetricAboutCurrentUserMessageCount(ActorInfo info)
    {
        var metricTagsSpecification = info.Context.GetType().GetField("_mailbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (metricTagsSpecification?.GetValue(info.Context) is not IMailbox mailbox) return;
        
        Console.WriteLine($"[{info.DeviceId}]: There are {mailbox.UserMessageCount} user messages pending to be processed");

        // TODO: use a different category (than ActorMailboxLength)
        ActorMetrics.ActorMailboxLength.Record(
            value: mailbox.UserMessageCount,
            tags: new KeyValuePair<string, object>[] { new("id", info.DeviceId), new("actortype", info.ActorType.Name) }
        );
    }

    private sealed record ActorInfo(ActorContext Context, string DeviceId, Type ActorType);

    private static async Task RunPeriodicActionRepeatedly<T>(Action<T> action, T actionParam, TimeSpan interval)
    {
        using PeriodicTimer timer = new(interval);
        while (true)
        {
            action(actionParam);
            await timer.WaitForNextTickAsync();
        }
    }
    */

}
