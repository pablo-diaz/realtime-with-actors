using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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
        Response.Headers.Append(key: "Grip-Hold", value: "stream");
        Response.Headers.Append(key: "Grip-Channel", value: DeviceStateConstants.Constants.ChannelNameForGeneralDeviceEventStream);
        Response.Headers.Append(key: "Access-Control-Allow-Origin", value: "*");

        return Task.FromResult<IActionResult>(Ok());
    }
}