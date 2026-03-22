using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartSure.ClaimsService.Data;
using SmartSure.ClaimsService.Services;
using SmartSure.Shared.Contracts.Extensions;
using System.Text;
using MassTransit;
using Serilog;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

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

// Serilog
builder.AddSerilogLogging("ClaimsService");

// Database
builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultClaimsConnection")));

// RabbitMQ (InMemory for testing)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SmartSure.ClaimsService.Consumers.PolicyActivatedConsumer>();
    x.AddConsumer<SmartSure.ClaimsService.Consumers.ClaimApprovedConsumer>();
    x.AddConsumer<SmartSure.ClaimsService.Consumers.ClaimRejectedConsumer>();

    x.UsingRabbitMq((ctx, cfg) => 
    {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

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
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

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

// Global Exception Handler
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

// Auto-migrate database and fix schema
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await db.Database.MigrateAsync();

    // Idempotent schema fix: ensure all columns exist in the database
    // This handles the case where migrations were recorded but columns were not actually created
    var schemaSql = @"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'ApprovedAmount')
            ALTER TABLE [Claims] ADD [ApprovedAmount] decimal(18,2) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'CreatedAt')
            ALTER TABLE [Claims] ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'RejectionReason')
            ALTER TABLE [Claims] ADD [RejectionReason] nvarchar(max) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Claims') AND name = 'UpdatedAt')
            ALTER TABLE [Claims] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'ContentType')
            ALTER TABLE [ClaimDocuments] ADD [ContentType] nvarchar(100) NOT NULL DEFAULT '';
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'FileSize')
            ALTER TABLE [ClaimDocuments] ADD [FileSize] bigint NOT NULL DEFAULT 0;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClaimDocuments') AND name = 'UploadedAt')
            ALTER TABLE [ClaimDocuments] ADD [UploadedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
    ";
    await db.Database.ExecuteSqlRawAsync(schemaSql);
}

app.Run();

