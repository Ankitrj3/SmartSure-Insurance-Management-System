using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using System.Text;
using Microsoft.OpenApi.Models;
using MassTransit;
using SmartSure.Shared.Contracts.Extensions;
using Serilog;
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.AddSerilogLogging("PolicyService");
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

// ==============================================================================
// 2. INFRASTRUCTURE & DATA ACCESS
// ==============================================================================

// Database
builder.Services.AddDbContext<PolicyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PolicyConnDb"));
    // Suppress pending model changes warning during startup
    options.ConfigureWarnings(warnings => 
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// ==============================================================================
// 3. MESSAGE BROKER (RabbitMQ & MassTransit)
// ==============================================================================

// RabbitMQ + MassTransit – auto-discover all consumers in this assembly
builder.Services.AddMassTransit(x =>
{
    // Scans the assembly and registers every class that implements IConsumer<T>
    // Currently discovers: UserRegisteredConsumer
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
// 4. AUTHENTICATION & SECURITY
// ==============================================================================

// Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
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

// Authorization
builder.Services.AddAuthorization();

// ==============================================================================
// 5. DEPENDENCY INJECTION (Services & Repositories)
// ==============================================================================

// Repositories
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();

// Services
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IPolicyMgmtService, PolicyMgmtService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IRazorpayService, RazorpayService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure Policy Service API", Version = "v1" });
    
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

    // Fix duplicate operationId errors from absolute route overrides
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(type => type.FullName);
});

var app = builder.Build();

// ==============================================================================
// 6. HTTP REQUEST PIPELINE (Middleware)
// ==============================================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled – gateway calls this service via HTTP
app.UseCorrelationId();
app.UseGlobalExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors("AllowGateway");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

