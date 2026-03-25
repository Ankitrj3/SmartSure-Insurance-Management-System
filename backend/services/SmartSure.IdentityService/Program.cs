using IdentityService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

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

// Internal Services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IdentityService.Repositories.IUserRepository, IdentityService.Repositories.UserRepository>();
builder.Services.AddScoped<IdentityService.Services.IAuthService, IdentityService.Services.AuthService>();
builder.Services.AddScoped<IdentityService.Services.IUserService, IdentityService.Services.UserService>();
builder.Services.AddScoped<IdentityService.Services.IEmailService, IdentityService.Services.EmailService>();
builder.Services.AddScoped<IdentityService.Services.IOtpService, IdentityService.Services.OtpService>();
builder.Services.AddScoped<IdentityService.Services.IGoogleAuthService, IdentityService.Services.GoogleAuthService>();
builder.Services.AddSingleton<IdentityService.Helpers.TokenService>();

// RabbitMQ configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<IdentityService.Consumers.ClaimStatusChangedConsumer>();

    x.UsingRabbitMq((ctx, cfg) => 
    {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnDb")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Identity Service API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
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

await AdminSeeder.SeedAdminAsync(app.Services, builder.Configuration);

app.Run();

