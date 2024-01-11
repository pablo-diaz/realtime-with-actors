using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jobs;

public class DeviceEventConsumer: IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private IServiceScope? _serviceScope = null;
    private Services.IMessageReceiver? _messageReceiver = null;

    public DeviceEventConsumer(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public void Dispose()
    {
        _messageReceiver?.Dispose();
        _serviceScope?.Dispose();
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _serviceScope = this._serviceProvider.CreateScope();
            _messageReceiver = _serviceScope.ServiceProvider.GetRequiredService<Services.IMessageReceiver>();
             var actorSystemConfig = _serviceScope.ServiceProvider.GetRequiredService<Infrastructure.ActorSystemConfiguration>();
            return _messageReceiver.StartReceivingMessages<Messages.DeviceEvent>(message => MessageProcessor.Process(message, actorSystemConfig));
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"[DeviceEventConsumer Job]: {ex.Message}");
            return Task.CompletedTask;
        }
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        this.Dispose();
        return Task.CompletedTask;
    }
}