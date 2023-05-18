using nigo.Services;
using nigo.Utility;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

addMyServices(builder);

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader());


app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void addMyServices(WebApplicationBuilder builder)
{
    //builder.Services.AddSingleton(new ApiService("E:\\Private\\wamp64\\www"));
    builder.Services.AddSingleton(new ApiService(Constants.csvFilePath));
    builder.Services.AddHostedService<BackgroundTask>();
    builder.Services.Configure<HostOptions>(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });
    //builder.Services.AddSingleton(new AppCache("localhost"));

   /* builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost";
        options.InstanceName = "MyRedisCacheInstance";
    });*/
}