using System.Text;
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

builder.Services.AddScoped<EventService>();
builder.Services.AddHttpClient();


builder.Services.AddDbContext<EventDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

//app.Use(async (context, next) => {
//    context.Request.EnableBuffering();

//    using (var reader = new StreamReader(
//        context.Request.Body,
//        encoding: Encoding.UTF8,
//        leaveOpen: true))
//    {
//        var body = await reader.ReadToEndAsync();
//        Console.WriteLine($"Request body: {body}");
//        context.Request.Body.Position = 0;
//    }

//    await next();
//});

app.MapControllers();

app.Run();
