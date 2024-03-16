using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/*
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Async(a => a.File(path: "event-publisher-requests-.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(10),
        rollingInterval: Serilog.RollingInterval.Hour))
    .CreateLogger();
*/

var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog();
builder.Services.AddControllers();

builder.Services.AddSingleton<Services.IMessageSender, Services.RabbitMqMessageSender>(sp =>
    new Services.RabbitMqMessageSender(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

var app = builder.Build();
//app.UseSerilogRequestLogging();
app.MapControllers();
app.Run();
