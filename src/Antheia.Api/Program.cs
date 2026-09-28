using Antheia.Api.Extensions;
using Antheia.Application;
using Antheia.Infrastructure;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Dima.WorkFlowAuditMiddleware.Data;
using Dima.WorkFlowAuditMiddleware.Extensions;
using Serilog;

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

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // Default to v1.0
    options.AssumeDefaultVersionWhenUnspecified = true; // Use default if client doesn't specify
    options.ReportApiVersions = true; // Returns "api-supported-versions" header in responses
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Reads version from URL /v1/
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Formats version groups as 'v1', 'v2', etc.
    options.SubstituteApiVersionInUrl = true; // Replaces {version:apiVersion} in route templates
});

// Register Swagger Generator with JWT & Dynamic Version Options
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseCorrelationId();
app.UseExceptionHandlingMiddleware();
app.UseSecurityHeaders();
app.UseHttpsRedirection();
app.UseCors(reactAppCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowAuditing();
app.MapControllers();

// Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Build an endpoint in the Swagger UI drop-down for each discovered API version
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Antheia API {description.GroupName.ToUpperInvariant()}"
            );
        }

        c.RoutePrefix = "swagger";
    });
}

try
{
    Log.Information("Starting web host");

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await AuditDbInitializer.InitializeAsync(dbContext);
    }

    await app.RunAsync();
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