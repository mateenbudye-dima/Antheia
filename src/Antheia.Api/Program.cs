using Antheia.Api.Extensions;
using Antheia.Application;
using Antheia.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
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
app.UseCustomExceptionHandler();
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

app.Run();