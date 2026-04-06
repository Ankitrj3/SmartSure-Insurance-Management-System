using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Services;
using SmartSure.ClaimsService.Repositories;
using SmartSure.Shared.Contracts.Extensions;
using System.Text;
using MassTransit;
using Serilog;
using SmartSure.ClaimsService.Extensions;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ==============================================================================
// 1. CONFIGURATION & ENVIRONMENT SETUP
// ==============================================================================

// CORS – allow Angular frontend and the API Gateway to call this service
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
builder.Configuration.AddEnvironmentVariables();

// ==============================================================================
// 2. LOGGING & OBSERVABILITY
// ==============================================================================

// Serilog
builder.AddSerilogLogging("ClaimsService");

// ==============================================================================
// 3. INFRASTRUCTURE & DATA ACCESS
// ==============================================================================

// Database
builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsConnDb")));

// ==============================================================================
// 4. MESSAGE BROKER (RabbitMQ & MassTransit)
// ==============================================================================

// RabbitMQ + MassTransit – auto-discover all consumers in this assembly
builder.Services.AddMassTransit(x =>
{
    // Scans the assembly and registers every class that implements IConsumer<T>
    // Currently discovers: PolicyActivatedConsumer, ClaimApprovedConsumer,
    //                      ClaimRejectedConsumer
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

// Authentication
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

// Services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Http Context Accessor
builder.Services.AddHttpContextAccessor();

// HTTP Client for cross-service calls
builder.Services.AddHttpClient();

// Repositories
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimStatusHistoryRepository, ClaimStatusHistoryRepository>();
builder.Services.AddScoped<IClaimDocumentRepository, ClaimDocumentRepository>();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure Claims Service API", Version = "v1" });

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
});

var app = builder.Build();

// ==============================================================================
// 7. HTTP REQUEST PIPELINE (Middleware)
// ==============================================================================

// Global Exception Handler
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
await app.ApplyMigrationsAsync();

app.Run();

