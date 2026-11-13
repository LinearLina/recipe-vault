using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RecipeVault.Api;
using RecipeVault.Api.Auth;
using RecipeVault.Api.Errors;
using RecipeVault.Application;
using RecipeVault.Infrastructure;
using RecipeVault.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:Default is not set. See README.md > Running it locally.");
}

builder.Services
    .AddControllers(options =>
        // Let missing fields reach our validator so it can return friendly messages.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        // snake_case keeps the JSON identical to the original Express API, so the React client is unchanged.
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = ApiErrors.FromModelState);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// `dotnet run -- --migrate` (optionally with --seed) prepares the database and exits.
if (args.Contains("--migrate"))
{
    await DatabaseInitializer.MigrateAsync(app.Services, seed: args.Contains("--seed"));
    return;
}

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHealthChecks("/api/health", new HealthCheckOptions { ResponseWriter = HealthResponseWriter.WriteAsync });
app.MapFallback(() => Results.NotFound(new ApiError("Not found")));

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API with WebApplicationFactory.</summary>
public partial class Program
{
}
