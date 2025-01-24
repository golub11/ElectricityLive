using nigo.Controllers;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();

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

//app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader());
//app.UseCors("AllowGoluxSoftWebApp"); //TODO
//app.UseAuthentication(); //TODO
app.UseAuthorization();

app.MapControllers();


app.Run();

static void addMyServices(WebApplicationBuilder builder)
{
    // TODO - start
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

    //builder.Services.AddAuthentication(options =>
    //{
    //    options.DefaultAuthenticateScheme = "ApiKeyScheme";
    //    options.DefaultChallengeScheme = "ApiKeyScheme";
    //}).AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKeyScheme", null);
    // TODO end
    builder.Services.AddTransient<DayAheadPricesController>();
    builder.Services.Configure<HostOptions>(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddHttpClient("MyApiClient", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}