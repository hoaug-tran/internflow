
using InternFlow.API.Middlewares;
using Serilog;

using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi






try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

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

    app.UseMiddleware<TraceIdMiddleware>();      // 1. gán traceId trước
    app.UseSerilogRequestLogging();              // 2. log request có TraceId
    app.UseMiddleware<ExceptionHandlingMiddleware>(); // 3. bắt lỗi

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();



    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



