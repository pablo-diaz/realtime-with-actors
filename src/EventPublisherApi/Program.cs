using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddSingleton<Services.IMessageSender, Services.RabbitMqMessageSender>(sp =>
    new Services.RabbitMqMessageSender(builder.Configuration.GetSection("RabbitMqConfig").Get<Services.Config.RabbitMqConfiguration>()));

var app = builder.Build();
app.MapControllers();
app.Run();
