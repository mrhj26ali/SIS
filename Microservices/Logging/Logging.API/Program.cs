using Logging.Infrastructure.Contexts;
using Logging.Infrastructure.Repositories;
using Logging.Application.Services;
using Logging.Application.IServices;
using Logging.Domain.IRepositories;
using Logging.API.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<LoggingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 2. DI Chain
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<Logging.Application.Profiles.LoggingProfile>();
});
// 3. MassTransit (CONSUMER)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LogMessageConsumer>();
    x.UsingRabbitMq((context, cfg) =>
{
    var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
    cfg.Host(host, "/", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });
    cfg.ConfigureEndpoints(context);
});
});
builder.Services.AddControllers();
// 4. Swagger & Endpoints
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIS Logging Microservice",
        Version = "v1",
        Description = "Centralized async logging service"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();