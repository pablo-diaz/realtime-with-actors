using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddSingleton<Services.IMessageReceiver, Services.RabbitMqMessageReceiver>(sp =>
    new Services.RabbitMqMessageReceiver(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

builder.Services.AddHostedService<Jobs.DeviceEventConsumer>();

var app = builder.Build();
app.MapControllers();
app.Run();
