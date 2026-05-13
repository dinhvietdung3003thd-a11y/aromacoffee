using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Nest;
using System.Data;
using System.Security.Claims;
using System.Text;
using WebApplication1.services;
using WebApplication1.services.interfaces;

// Configure Dapper to automatically map snake_case database columns to PascalCase C# properties
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// 1. Register all services for dependency injection (scoped lifetime: new instance per HTTP request)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenVersionValidator, TokenVersionValidator>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddAuthorization();

// 2. Configure MySQL database connection (Dapper ORM)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>((sp) => new MySqlConnection(connectionString));

// 3. Configure JWT authentication and token validation
var secretKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(secretKey))
    throw new Exception("Missing Jwt:Key");
// Convert secret key to UTF-8 bytes for HMAC signature verification
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateLifetime = true,
            // Disable clock skew to enforce strict token expiration (prevent 401 errors from time drift)
            ClockSkew = TimeSpan.Zero
        };

        // Custom token validation: verify token version hasn't been invalidated
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                // Extract claims from JWT token
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleClaim = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                var tokenVersionClaim = context.Principal?.FindFirst("tokenVersion")?.Value;

                // Validate all required claims are present and parseable
                if (!int.TryParse(userIdClaim, out var actorId) ||
                    string.IsNullOrWhiteSpace(roleClaim) ||
                    !int.TryParse(tokenVersionClaim, out var tokenVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                // Verify token version matches database version (logout-all mechanism)
                var validator = context.HttpContext.RequestServices.GetRequiredService<ITokenVersionValidator>();
                var isValid = await validator.ValidateAsync(actorId, roleClaim, tokenVersion);

                if (!isValid)
                {
                    context.Fail("Token is no longer valid.");
                }
            }
        };
    });

// --- 4. Cấu hình CORS ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// 5. Configure Elasticsearch for product search functionality
// Elasticsearch provides fast full-text search independent of MySQL queries
var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var settings = new ConnectionSettings(new Uri(esUri))
    .DefaultIndex(builder.Configuration["Elasticsearch:DefaultIndex"] ?? "aroma_products")
    .EnableApiVersioningHeader();
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập Token của bạn vào đây (Không cần gõ chữ Bearer, chỉ cần dán mã Token)."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --- 6.Hubs ---
builder.Services.AddSignalR();  

var app = builder.Build();
app.MapHub<WebApplication1.Hubs.OrderHub>("/orderHub");

// --- 5. Pipeline Middleware  ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendCors");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
