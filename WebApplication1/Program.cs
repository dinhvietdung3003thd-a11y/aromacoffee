using MySqlConnector;
using System.Data;
using WebApplication1.services;
using WebApplication1.services.interfaces;
using WebApplication1.Services;
using WebApplication1.Services.interfaces;
using Nest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ITableService, TableService>();

builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký IDbConnection để sử dụng Dapper (Sử dụng Scoped để tối ưu bộ nhớ)
builder.Services.AddScoped<IDbConnection>((sp) => new MySqlConnection(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Lấy thông tin từ appsettings.json
var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var defaultIndex = builder.Configuration["Elasticsearch:DefaultIndex"] ?? "aroma_products";

var settings = new ConnectionSettings(new Uri(esUri))
    .DefaultIndex(defaultIndex)
    .EnableApiVersioningHeader(); 

var client = new ElasticClient(settings);

//3. Đăng ký Singleton để sử dụng trong các Service
builder.Services.AddSingleton<IElasticClient>(client);

// Cấu hình Swagger để xem tài liệu API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();