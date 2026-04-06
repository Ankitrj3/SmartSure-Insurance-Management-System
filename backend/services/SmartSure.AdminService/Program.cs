using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.AdminService.Data;
using SmartSure.AdminService.Services;
using SmartSure.AdminService.Repositories;
using SmartSure.Shared.Contracts.Extensions;
using System.Text;
using MassTransit;
using Serilog;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ==============================================================================
// 1. CONFIGURATION & ENVIRONMENT SETUP
// ==============================================================================

// CORS Configuration
// Allows cross-origin requests from the Angular frontend and API Gateway.
// Essential for enabling secure communication between remote clients and this service.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5057",
                "https://localhost:9000"
              )
              .AllowAnyHeader()
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .AllowCredentials());
});

// ==============================================================================
// 2. LOGGING & OBSERVABILITY
// ==============================================================================

// Configure Serilog for structured logging across the application.
builder.AddSerilogLogging("AdminService");

// Cross-service calls (HTTP – no TLS bypass needed for local dev)
builder.Services.AddHttpClient("IdentityClient");
builder.Services.AddHttpClient();

// ==============================================================================
// 3. INFRASTRUCTURE & DATA ACCESS
// ==============================================================================

// Register Entity Framework Core DbContext
// Uses SQL Server with connection string provided via environment variables.
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AdminConnDb")));

// ==============================================================================
// 4. MESSAGE BROKER (RabbitMQ & MassTransit)
// ==============================================================================

// Configure asynchronous messaging for event-driven communication.
// Auto-discovers consumers and registers their respective queues.
builder.Services.AddMassTransit(x =>
{
    // Scans the assembly and registers every class that implements IConsumer<T>
    // Currently discovers: UserRegisteredConsumer, PolicyActivatedConsumer,
    //                      PolicyCancelledConsumer, ClaimSubmittedConsumer,
    //                      ClaimStatusChangedConsumer
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Auto-creates one RabbitMQ queue/exchange per registered consumer
        cfg.ConfigureEndpoints(ctx);
    });
});

// ==============================================================================
// 5. AUTHENTICATION & SECURITY
// ==============================================================================

// JWT Bearer Token Configuration
// Validates incoming tokens against the configured Issuer, Audience, and Key.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudiences = new[] { "Aud1", "Aud2", "Aud3", "Aud4", "Aud5" }.Select(k => builder.Configuration[$"Jwt:{k}"]).Where(v => !string.IsNullOrEmpty(v)).ToArray(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization();

// ==============================================================================
// 6. DEPENDENCY INJECTION (Services & Repositories)
// ==============================================================================

// Register core business services with Scoped lifetime (one instance per HTTP request).
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();

// Repositories
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure Admin Reporting Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Fix duplicate operationId / schema conflicts
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// ==============================================================================
// 7. HTTP REQUEST PIPELINE (Middleware)
// ==============================================================================

// Global Exception Handler matches raw exceptions to standard HTTP responses.
// Serilog tracks requests for performance and error auditing.
app.UseCorrelationId();
app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled – gateway calls this service via HTTP
app.UseCors("AllowGateway");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

