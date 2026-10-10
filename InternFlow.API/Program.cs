using System.Text;
using InternFlow.API.Conventions;
using InternFlow.API.Extensions;
using InternFlow.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    LoadEnvironmentVariables();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext(),
        preserveStaticLogger: true);

    // Add services to the container.
    builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new ApiResponseConvention());
});
    builder.Services.AddEndpointsApiExplorer();

    // Cấu hình Swagger với JWT Bearer Authorize
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "InternFlow API",
            Version = "v1",
            Description = "Hệ thống Quản lý Thực tập & Phân công Công việc"
        });


        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập Access Token JWT theo định dạng: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        options.OperationFilter<InternFlow.API.Swagger.ErrorResponsesOperationFilter>();
        foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "InternFlow.*.xml"))
            options.IncludeXmlComments(xml);
    });

    // 1. Cấu hình DbContext, MediatR, HealthChecks và các Service dự án
    builder.Services.AddProjectServices(builder.Configuration);

    // 2. Cấu hình JWT Authentication
    var jwtSecret = builder.Configuration["Jwt:Secret"]
                    ?? builder.Configuration["Jwt:Key"]
                    ?? "InternFlow_Super_Secret_Jwt_Signing_Key_For_Identity_Auth_2026!";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InternFlow";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "InternFlowClient";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Pipeline Middlewares
    app.UseMiddleware<TraceIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {

            c.SwaggerEndpoint("/swagger/v1/swagger.json", "InternFlow API v1");
        });
        await app.MigrateAndSeedAsync();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

    app.MapControllers();

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

public partial class Program
{
    private static void LoadEnvironmentVariables()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var envPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    {
                        continue;
                    }

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var val = parts[1].Trim().Trim('"', '\'');
                        Environment.SetEnvironmentVariable(key, val);
                    }
                }
                break;
            }
            dir = dir.Parent;
        }

        var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "InternFlowDb";
        if (!string.IsNullOrEmpty(saPassword))
        {
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection",
                $"Server=localhost,1433;Database={dbName};User Id=sa;Password={saPassword};TrustServerCertificate=True;");
        }

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            Environment.SetEnvironmentVariable("Jwt__Secret", jwtSecret);
        }

        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        if (!string.IsNullOrEmpty(jwtIssuer))
        {
            Environment.SetEnvironmentVariable("Jwt__Issuer", jwtIssuer);
        }

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        if (!string.IsNullOrEmpty(jwtAudience))
        {
            Environment.SetEnvironmentVariable("Jwt__Audience", jwtAudience);
        }
    }
}
