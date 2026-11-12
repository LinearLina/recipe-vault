using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RecipeVault.Api;

public static class HealthResponseWriter
{
    /// <summary>Writes {"status":"ok"} like the original API, but only when the database is reachable.</summary>
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(report);

        var status = report.Status == HealthStatus.Healthy ? "ok" : "unhealthy";
        return context.Response.WriteAsJsonAsync(new { status });
    }
}
