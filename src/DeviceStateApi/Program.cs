using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddActorSystem();

builder.Services.AddSingleton<Services.IMessageReceiver, Services.RabbitMqMessageReceiver>(sp =>
    new Services.RabbitMqMessageReceiver(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

builder.Services.AddSingleton<DeviceStateServices.IUserEventPublisher, Infrastructure.PushpinSseUserEventPublisher>(sp =>
    new PushpinSseUserEventPublisher(config: builder.Configuration.GetSection("PushpinSetup").Get<Infrastructure.PushpinConfig>()!));

builder.Services.AddSingleton<DeviceStateServices.IEventStore, Infrastructure.InlfuxDbEventStore>(sp =>
    new InlfuxDbEventStore(config: builder.Configuration.GetSection("InfluxDbSetup").Get<Infrastructure.InfluxDbConfig>()!));

builder.Services.AddHostedService<Jobs.DeviceEventConsumer>();

var app = builder.Build();
app.MapControllers();
app.Run();
