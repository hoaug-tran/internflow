using InternFlow.API.Middlewares;
using Serilog;



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

