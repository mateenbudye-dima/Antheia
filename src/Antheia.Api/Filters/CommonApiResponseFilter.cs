using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class CommonApiResponseFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ensure operation and responses dictionary are initialized
        if (operation == null) return;

        operation.Responses ??= new OpenApiResponses();

        // Automatically add 400 Bad Request to POST, PUT, PATCH
        if (context.ApiDescription.HttpMethod is "POST" or "PUT" or "PATCH")
        {
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
        }

        // Automatically add 500 Internal Server Error to all endpoints
        operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });

        // Add 401 Unauthorized if endpoint requires authorization
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Any() ?? false;

        if (hasAuthorize)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        }
    }
}