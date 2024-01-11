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
            return _messageReceiver.StartReceivingMessages<Messages.DeviceEvent>(ProcessMessage);
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine($"[DeviceEventConsumer Job]: {ex.Message}");
            return Task.CompletedTask;
        }
    }

    private Task ProcessMessage(Messages.DeviceEvent @event)
    {
        Console.WriteLine($"Nuevo mensaje: DevId: '{@event.DeviceId}'");
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        this.Dispose();
        return Task.CompletedTask;
    }
}