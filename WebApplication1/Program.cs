using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Nest;
using System.Data;
using System.Security.Claims;
using System.Text;
using WebApplication1.services;
using WebApplication1.services.interfaces;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Đăng ký các Service (Dependency Injection) ---
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

// --- 2. Cấu hình Database (MySQL + Dapper) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>((sp) => new MySqlConnection(connectionString));

// --- 3. Cấu hình Authentication ---
var secretKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(secretKey))
    throw new Exception("Missing Jwt:Key");
var key = Encoding.UTF8.GetBytes(secretKey); // Thống nhất dùng UTF8

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
            ClockSkew = TimeSpan.Zero // Quan trọng: Tránh lệch múi giờ gây lỗi 401 ngay lập tức
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleClaim = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                var tokenVersionClaim = context.Principal?.FindFirst("tokenVersion")?.Value;

                if (!int.TryParse(userIdClaim, out var actorId) ||
                    string.IsNullOrWhiteSpace(roleClaim) ||
                    !int.TryParse(tokenVersionClaim, out var tokenVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

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
// --- 4. Cấu hình Elasticsearch ---
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
