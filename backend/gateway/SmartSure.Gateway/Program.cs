using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using SmartSure.Shared.Contracts.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.AddSerilogLogging("SmartSure.Gateway");

// Add Ocelot configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add HttpClient and Ocelot with Swagger aggregation
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5057",
                "https://localhost:9000"
              )
              .AllowAnyHeader()
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .AllowCredentials();
    });
});

var app = builder.Build();

// Global Exception Handler
app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // SwaggerForOcelotUI aggregates all downstream service docs into one UI
    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs";
        options.DocumentTitle = "SmartSure API Gateway";
    });
}

// app.UseHttpsRedirection(); // Commented out to prevent CORS preflight redirect errors with HTTP locally

app.UseCors("AllowAngularApp");


// Use Ocelot middleware (last)
await app.UseOcelot();

app.Run();
