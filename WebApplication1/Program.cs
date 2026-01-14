using MySqlConnector;
using System.Data;
// Hãy thay 'WebApplication1' bằng namespace thực tế của bạn nếu khác
using WebApplication1.Models;
using WebApplication1.services;
using WebApplication1.services.interfaces;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICatagoryService, CatagoryService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ITableService, TableService>();
// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký IDbConnection để sử dụng Dapper (Sử dụng Scoped để tối ưu bộ nhớ)
builder.Services.AddScoped<IDbConnection>((sp) => new MySqlConnection(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cấu hình Swagger để 2 bạn FE xem tài liệu API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();