using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext (tạm thời cấu hình chuỗi kết nối hoặc in-memory nếu chưa có DB thật)
builder.Services.AddDbContext<InternFlowDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                           ?? "Server=localhost;Database=InternFlowDb;Trusted_Connection=True;TrustServerCertificate=True;";
    options.UseSqlServer(connectionString);
});

// 2. Đăng ký các Service bàn giao cho team
builder.Services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
builder.Services.AddScoped<IInternshipQueryService, InternshipQueryService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();