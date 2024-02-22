using nigo.Services;
using nigo.Utility;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using nigo.Controllers;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();

builder.Services.AddCors();// TODO
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

addMyServices(builder);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader());
//app.UseCors("AllowGoluxSoftWebApp"); //TODO
//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

static void addMyServices(WebApplicationBuilder builder)
{
    //var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Value.Split(',');
    //builder.Services.AddCors(options =>
    //{
    //    options.AddPolicy("AllowGoluxSoftWebApp",
    //        builder =>
    //        {
    //            builder.WithOrigins(allowedOrigins)
    //                .AllowAnyHeader()
    //                .AllowAnyMethod();
    //        });
    //});

    builder.Services.AddAuthentication(options =>
     {
         options.DefaultAuthenticateScheme = "ApiKeyScheme";
         options.DefaultChallengeScheme = "ApiKeyScheme";
     }).AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", null);

    builder.Services.AddTransient<DayAheadPricesController>();
    builder.Services.AddTransient<ApiService>();
    builder.Services.AddHostedService<BackgroundTask>();
    builder.Services.Configure<HostOptions>(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddHttpClient("MyApiClient", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}