using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class EventController: ControllerBase
{
    private readonly IMessageSender _broker;

    public EventController(IMessageSender broker)
    {
        this._broker = broker;
    }

    [HttpPost]
    public async Task<IActionResult> TrackNewDeviceEvent([FromBody] Controllers.DTOs.DeviceEvent @event)
    {
        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        await _broker.SendMessage(new Messages.DeviceEvent {
            At = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            DeviceId = @event.DevId,
            Temperature = @event.Temp,
            Latitude = @event.Lat,
            Longitude = @event.Lon
        });

        stopWatch.Stop();

        System.Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {Request.Headers["User-Agent"]}] Time publishing to broker: {stopWatch.Elapsed.Seconds:00}.{stopWatch.Elapsed.Milliseconds:00}");

        return Accepted();
    }
}