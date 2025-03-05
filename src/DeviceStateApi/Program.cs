using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DeviceStateApi.Services;
using DeviceStateApi.Infrastructure.ServiceImpl;

using Infrastructure;
using Infrastructure.ServiceImpl;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

using Proto.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
ConfigureMetrics(builder);
builder.Services.AddControllers();
ConfigureApplicationServices(builder);
ConfigureEventStoreTechnology(builder);
ConfigureBackgroundJobs(builder);

var app = builder.Build();
app.MapControllers();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();

static void ConfigureMetrics(WebApplicationBuilder builder)
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(b => b
            .AddService(serviceName: "ActorModelSystem")
            //.AddDetector( IResourceDetector
            .Build())
        .WithMetrics(b => b
            .AddPrometheusExporter()
            .AddProtoActorInstrumentation());
}

static void ConfigureApplicationServices(WebApplicationBuilder builder)
{
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

    builder.Services.AddActorSystem(
        withSetup: builder.Configuration.GetSection("DeviceMonitoringSetup").Get<DeviceStateModel.Config.DeviceMonitoringSetup>()!);

    builder.Services.AddSingleton<Services.IMessageReceiver, Services.RabbitMqMessageReceiver>(sp =>
        new Services.RabbitMqMessageReceiver(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

    builder.Services.AddSingleton<DeviceStateServices.IUserEventPublisher, PushpinSseUserEventPublisher>(sp =>
        new PushpinSseUserEventPublisher(config: builder.Configuration.GetSection("PushpinSetup").Get<PushpinConfig>()!));
}

static void ConfigureEventStoreTechnology(WebApplicationBuilder builder)
{
    var eventStoreImplementationTechnology = builder.Configuration.GetSection("EventStoreImplementationTech").Value;

    if ("KurrentDb" == eventStoreImplementationTechnology)
    {
        builder.Services.AddOptions<KurrentDbConfig>().Bind(builder.Configuration.GetSection("KurrentDbSetup"));
        builder.Services.AddSingleton<IEventStore, KurrentDbEventStore>();
        builder.Services.AddSingleton<IQueryServiceForEventStore, QueryServiceForEventStoreBasedOnKurrentDb>();
    }
    else if ("InfluxDb" == eventStoreImplementationTechnology)
    {
        builder.Services.AddSingleton<IEventStore, InlfuxDbEventStore>(sp =>
            new InlfuxDbEventStore(config: builder.Configuration.GetSection("InfluxDbSetup").Get<InfluxDbConfig>()!));

        builder.Services.AddSingleton<IQueryServiceForEventStore, QueryServiceForEventStoreBasedOnInfluxDb>(sp =>
            new QueryServiceForEventStoreBasedOnInfluxDb(config: builder.Configuration.GetSection("InfluxDbSetup").Get<InfluxDbConfig>()!));
    }
}

static void ConfigureBackgroundJobs(WebApplicationBuilder builder)
{
    builder.Services.AddHostedService<Jobs.DeviceEventConsumer>();
}
