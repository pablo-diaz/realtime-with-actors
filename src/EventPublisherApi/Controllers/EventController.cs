using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Controllers.DTOs;
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
    public async Task<IActionResult> TrackNewDeviceEvent([FromBody] DeviceEvent @event)
    {
        await _broker.SendMessage(new {
            At = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            DeviceId = @event.DevId,
            Temperature = @event.Temp,
            Latitude = @event.Lat,
            Longitude = @event.Lon
        });

        return Accepted();
    }
}