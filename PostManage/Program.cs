using Microsoft.EntityFrameworkCore;
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

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<IPostService, PostService>();

// Add CORS
builder.Services.AddCors(options =>
{
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
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
