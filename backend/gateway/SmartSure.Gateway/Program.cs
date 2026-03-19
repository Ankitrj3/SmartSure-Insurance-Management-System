using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Ocelot configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add HttpClient and Ocelot with Swagger aggregation
builder.Services.AddHttpClient();
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Important: SwaggerForOcelotUI must be BEFORE UseOcelot
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/v1/swagger.json";
    });
}

app.UseHttpsRedirection();

// Gateway health check
app.MapGet("/", () => Results.Ok(new { message = "SmartSure API Gateway is healthy", status = "Active" }));

// Use Ocelot middleware (last)
await app.UseOcelot();

app.Run();
