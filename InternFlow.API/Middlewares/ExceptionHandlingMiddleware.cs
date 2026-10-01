using InternFlow.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning("Handled exception ({Status}): {Message}", status, ex.Message);

            if (context.Response.HasStarted) throw;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = status >= 500 ? "Internal Server Error" : ex.GetType().Name.Replace("Exception", ""),
                Detail = status >= 500 && !_env.IsDevelopment() ? "Đã có lỗi xảy ra." : ex.Message,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null,
                "application/problem+json");
        }
    }
}
