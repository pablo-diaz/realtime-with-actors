using System.Threading.Tasks;

using DeviceStateApi.Utils;
using DeviceStateModel.Device;

using Proto;
using Proto.Mailbox;

namespace DeviceStateApi.DeviceStateModel.Device;

public class MetricsReporterActor : IActor
{
    public const string ActorNameInRegistry = "ActorForMetricsReportingLogic";

    public Task ReceiveAsync(IContext context) => context.Message switch
    {
        WatchInboxLengthRequest message => Handle(message),
        _ => Task.CompletedTask
    };

    private Task Handle(WatchInboxLengthRequest message)
    {
        var metricTagsSpecification = message.Context.GetType().GetField("_mailbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (metricTagsSpecification?.GetValue(message.Context) is not IMailbox mailbox) return Task.CompletedTask;

        OpenTelemetryMetricsHelper.ReportMetric(ofCurrentLength: new(Id: message.Id, CurrentLengthOfInboxQueue: mailbox.UserMessageCount, ActorType: message.ActorType));

        return Task.CompletedTask;
    }

}
