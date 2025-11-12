using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using PostManage.Data;
using PostManage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Post Management API",
        Version = "v1",
        Description = "API for managing posts with CRUD operations",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Post Management API"
        }
    });
});

// Add DbContext with connection string normalization
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// Normalize connection string (handles whitespace and special characters)
var normalizedConnectionString = connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                                 connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
    ? ConvertUriToConnectionString(connectionString)
    : NormalizeKeyValueConnectionString(connectionString);

// Validate connection string
try
{
    var _ = new NpgsqlConnectionStringBuilder(normalizedConnectionString);
}
catch (Exception ex)
{
    throw new InvalidOperationException($"Invalid connection string format: {ex.Message}", ex);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(normalizedConnectionString));

// Add services
builder.Services.AddScoped<IPostService, PostService>();

// Add CORS
builder.Services.AddCors(options =>
{
    // Policy for frontend (Vercel)
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://post-management-ui.vercel.app",  // Production Vercel URL
                "http://localhost:3000",                    // Local development
                "http://localhost:5000",                    // Alternative local port
                "http://localhost:5173"                     // Vite dev server
            )
            .AllowAnyMethod()  // GET, POST, PUT, DELETE, OPTIONS
            .AllowAnyHeader()  // Content-Type, Authorization, etc.
            .AllowCredentials(); // Allow cookies and credentials
    });
    
    // Fallback policy for other origins (Swagger, etc.)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Post Management API v1");
    c.RoutePrefix = "swagger";
});

// Render handles HTTPS termination, so we use HTTP only
// Port is set via ASPNETCORE_URLS environment variable (configured in entrypoint.sh)

// Use CORS middleware BEFORE MapControllers
// This allows preflight OPTIONS requests to be handled correctly
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

static string ConvertUriToConnectionString(string uriString)
{
    var uri = new Uri(uriString);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = uri.UserInfo.Split(':')[0],
        Password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : string.Empty
    };
    
    if (!string.IsNullOrEmpty(uri.Query))
    {
        var query = uri.Query.TrimStart('?');
        foreach (var param in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = param.Split('=', 2);
            if (parts.Length == 2 && parts[0].Trim().ToLowerInvariant() == "sslmode")
            {
                if (Enum.TryParse<SslMode>(Uri.UnescapeDataString(parts[1].Trim()), true, out var sslMode))
                {
                    builder.SslMode = sslMode;
                }
            }
        }
    }
    
    return builder.ConnectionString;
}

static string NormalizeKeyValueConnectionString(string rawConnectionString)
{
    var segments = rawConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
    var sb = new StringBuilder();

    foreach (var segment in segments)
    {
        var trimmedSegment = segment.Trim();
        if (string.IsNullOrWhiteSpace(trimmedSegment) || !trimmedSegment.Contains('='))
            continue;

        var parts = trimmedSegment.Split('=', 2);
        if (parts.Length != 2)
            continue;

        var key = parts[0].Trim().Trim('"', '\'');
        var value = parts[1].Trim().Trim('"', '\'');

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            continue;

        if (sb.Length > 0)
            sb.Append(';');

        sb.Append(key).Append('=').Append(value);
    }

    return sb.ToString();
}
