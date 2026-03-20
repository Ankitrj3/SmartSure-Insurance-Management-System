using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSure.PolicyService.Data;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using System.Text;
using Microsoft.OpenApi.Models;
using MassTransit;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Database
builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultPolicyConnection")));

// MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SmartSure.PolicyService.Consumers.UserRegisteredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"]!, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

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
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Authorization
builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<IInsuranceRepository, InsuranceRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<IPolicyMgmtService, PolicyMgmtService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddControllers();

// Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartSure Policy Service API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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
