using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Presentation.Data;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var serviceBusConnection = builder.Configuration["ServiceBusConnection"];
builder.Services.AddSingleton(x => new ServiceBusClient(serviceBusConnection));
builder.Services.AddSingleton(x =>
    x.GetRequiredService<ServiceBusClient>().CreateSender("packages-bus"));
builder.Services.AddSingleton<EventBusListener>();
builder.Services.AddHostedService<EventBusListener>();

builder.Services.AddScoped<EventService>();

builder.Services.AddDbContext<EventDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();
