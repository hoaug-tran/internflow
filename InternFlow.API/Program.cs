using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using InternFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Cấu hình DbContext
builder.Services.AddDbContext<InternFlowDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? "Server=localhost;Database=InternFlowDb;Trusted_Connection=True;TrustServerCertificate=True;";
    options.UseSqlServer(connectionString);
});

// 2. Đăng ký các Service bàn giao cho team
builder.Services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
builder.Services.AddScoped<IInternshipQueryService, InternshipQueryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
