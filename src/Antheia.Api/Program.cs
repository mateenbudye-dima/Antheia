using Antheia.Api.Extensions;
using Antheia.Application;
using Antheia.Infrastructure;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from configuration (appsettings.json)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
const string reactAppCorsPolicy = "AllowReactApp";

builder.Services.AddApplicationServices()
                .AddInfrastructureServices(builder.Configuration)
                .AddJwtAuthentication(builder.Configuration)
                .AddCorsPolicy(builder.Configuration, reactAppCorsPolicy);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseCorrelationId();
app.UseExceptionHandlingMiddleware();
app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseCors(reactAppCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = "swagger";
    });
}

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
