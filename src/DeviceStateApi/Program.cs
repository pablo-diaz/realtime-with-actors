using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DeviceStateApi.Services;

using Infrastructure;
using Infrastructure.ServiceImpl;
using DeviceStateApi.Infrastructure.ServiceImpl;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddActorSystem(
    withSetup: builder.Configuration.GetSection("DeviceMonitoringSetup").Get<DeviceStateModel.Config.DeviceMonitoringSetup>()!);

builder.Services.AddSingleton<Services.IMessageReceiver, Services.RabbitMqMessageReceiver>(sp =>
    new Services.RabbitMqMessageReceiver(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

builder.Services.AddSingleton<DeviceStateServices.IUserEventPublisher, PushpinSseUserEventPublisher>(sp =>
    new PushpinSseUserEventPublisher(config: builder.Configuration.GetSection("PushpinSetup").Get<PushpinConfig>()!));

builder.Services.AddSingleton<IEventStore, InlfuxDbEventStore>(sp =>
    new InlfuxDbEventStore(config: builder.Configuration.GetSection("InfluxDbSetup").Get<InfluxDbConfig>()!));

builder.Services.AddSingleton<IQueryServiceForEventStore, QueryServiceForEventStoreBasedOnInfluxDb>(sp =>
    new QueryServiceForEventStoreBasedOnInfluxDb(config: builder.Configuration.GetSection("InfluxDbSetup").Get<InfluxDbConfig>()!));

builder.Services.AddHostedService<Jobs.DeviceEventConsumer>();

var app = builder.Build();
app.MapControllers();
app.Run();
