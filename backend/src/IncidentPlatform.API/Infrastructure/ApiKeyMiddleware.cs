namespace IncidentPlatform.API.Infrastructure;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Allow CORS preflight, health checks, root, and Swagger without API key
        if (context.Request.Method == HttpMethods.Options ||
            path == "/" ||
            path == "/health" ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var configuredKey = context.RequestServices
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("ApiKey");

        // If no API key is configured, skip validation (local dev without key)
        if (string.IsNullOrEmpty(configuredKey))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
            !string.Equals(providedKey, configuredKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or missing API key." });
            return;
        }

        await _next(context);
    }
}
