using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class UserSubscriptionController: ControllerBase
{
    [HttpGet("toGeneralDeviceEventStream")]
    public Task<IActionResult> SubscribeUserToGeneralDeviceEventStream()
    {
        // https://pushpin.org/docs/usage/#server-sent-events

        Response.ContentType = "text/event-stream";  // Server-sent events
        Response.Headers.Add(key: "Grip-Hold", value: "stream");
        Response.Headers.Add(key: "Grip-Channel", value: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream);

        return Task.FromResult<IActionResult>(Ok());
    }
}