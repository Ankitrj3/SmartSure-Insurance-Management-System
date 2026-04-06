using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using MMLib.SwaggerForOcelot.DependencyInjection;
using SmartSure.Shared.Contracts.Extensions;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────────────
builder.AddSerilogLogging("SmartSure.Gateway");

// ── Ocelot config file ─────────────────────────────────────────────────────
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ── Health Checks (ASP.NET Core built-in, no extra NuGet needed) ───────────
builder.Services.AddHealthChecks()
    .AddCheck("SmartSure Gateway", () => HealthCheckResult.Healthy("SmartSure Gateway API is running."));

// ── Ocelot + Polly + Swagger ───────────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOcelot(builder.Configuration).AddPolly();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// ── CORS ───────────────────────────────────────────────────────────────────
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

// ── Global Exception Handler + Serilog request logging ────────────────────
app.UseCorrelationId();
app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();

// ── Dev tools ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs";
    });
}

// app.UseHttpsRedirection(); // Commented out to prevent CORS preflight redirect errors with HTTP locally

app.UseCors("AllowAngularApp");

// ── Health Check endpoint (MUST be before UseOcelot – Ocelot is terminal) ──
//    Accessible at: http://localhost:5057/health  OR  https://localhost:9000/health
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status  = report.Status.ToString(),
            service = "SmartSure Gateway",
            message = "SmartSure Gateway API is running.",
            url     = $"{context.Request.Scheme}://{context.Request.Host}",
            checks  = report.Entries.Select(e => new
            {
                name        = e.Key,
                status      = e.Value.Status.ToString(),
                description = e.Value.Description
            }),
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { WriteIndented = true }));
    }
});

// ── Minimal API – simple alive probe ──────────────────────────────────────
//    GET /  →  plain-text "SmartSure Gateway API is running."
//    Useful as a quick browser check at https://localhost:9000
app.MapGet("/", () => Results.Ok(new
{
    status  = "Healthy",
    service = "SmartSure Gateway",
    message = "SmartSure Gateway API is running.",
    version = "1.0",
    timestamp = DateTime.UtcNow
}))
.WithName("GatewayStatus")
.WithSummary("Gateway liveness probe");

// ── Routing & Endpoints (Must be evaluated BEFORE Ocelot) ─────────────────
// By explicitly invoking UseRouting and UseEndpoints here, any matched Minimal API
// (like GET / or /health) will execute and TERMINATE the request, preventing Ocelot 
// from erroneously proxying it and throwing the 'ReasonPhrase' conflict exception.
app.UseRouting();
app.UseEndpoints(endpoints => { });

// ── Ocelot middleware (terminal for all other proxy routes) ────────────────
await app.UseOcelot();

app.Run();
