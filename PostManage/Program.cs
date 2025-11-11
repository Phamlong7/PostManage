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

// Add DbContext with safe connection string handling
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Handle connection string - Render may provide URI format or standard format
string normalizedConnectionString;

// If connection string is a URI (starts with postgres:// or postgresql://), convert it
if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        var uri = new Uri(connectionString);
        var connectionBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = uri.UserInfo.Split(':')[0],
            Password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : string.Empty
        };
        
        // Parse query string for additional parameters
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = uri.Query.TrimStart('?');
            var queryParams = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in queryParams)
            {
                var parts = param.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim().ToLowerInvariant();
                    var value = Uri.UnescapeDataString(parts[1].Trim());
                    
                    if (key == "sslmode" && Enum.TryParse<SslMode>(value, true, out var sslMode))
                    {
                        connectionBuilder.SslMode = sslMode;
                    }
                }
            }
        }
        
        normalizedConnectionString = connectionBuilder.ConnectionString;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to parse PostgreSQL URI connection string: {ex.Message}", ex);
    }
}
else
{
    normalizedConnectionString = NormalizeKeyValueConnectionString(connectionString);
}

// Validate and parse connection string using NpgsqlConnectionStringBuilder
NpgsqlConnectionStringBuilder connectionStringBuilder;
try
{
    connectionStringBuilder = new NpgsqlConnectionStringBuilder(normalizedConnectionString);
}
catch (Exception ex)
{
    // Log connection string info for debugging (mask password)
    var maskedConnectionString = normalizedConnectionString;
    if (normalizedConnectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
    {
        var passwordIndex = normalizedConnectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
        var passwordStart = passwordIndex + "Password=".Length;
        var passwordEnd = normalizedConnectionString.IndexOf(';', passwordStart);
        if (passwordEnd == -1) passwordEnd = normalizedConnectionString.Length;
        maskedConnectionString = normalizedConnectionString.Substring(0, passwordStart) + 
                               "***" + 
                               normalizedConnectionString.Substring(passwordEnd);
    }
    
    throw new InvalidOperationException(
        $"Invalid connection string format at index {ex.Message}. " +
        $"Connection string length: {normalizedConnectionString.Length}. " +
        $"Masked connection string: {maskedConnectionString}", ex);
}

// Log connection info (without password) for debugging
var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
logger.LogInformation("Database connection configured: Host={Host}, Database={Database}, Username={Username}, Port={Port}",
    connectionStringBuilder.Host,
    connectionStringBuilder.Database,
    connectionStringBuilder.Username,
    connectionStringBuilder.Port);

// Use the validated connection string
normalizedConnectionString = connectionStringBuilder.ConnectionString;

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

static string NormalizeKeyValueConnectionString(string rawConnectionString)
{
    var segments = rawConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
    var sb = new StringBuilder();

    foreach (var segment in segments)
    {
        var trimmedSegment = segment.Trim();
        if (string.IsNullOrWhiteSpace(trimmedSegment) || !trimmedSegment.Contains('='))
        {
            continue;
        }

        var parts = trimmedSegment.Split('=', 2);
        if (parts.Length != 2)
        {
            continue;
        }

        var key = parts[0].Trim().Trim('"', '\'');
        var value = parts[1].Trim().Trim('"', '\'');

        if (string.IsNullOrEmpty(key))
        {
            continue;
        }

        if (string.IsNullOrEmpty(value))
        {
            continue;
        }

        if (sb.Length > 0)
        {
            sb.Append(';');
        }

        sb.Append(key);
        sb.Append('=');
        sb.Append(value);
    }

    return sb.ToString();
}
