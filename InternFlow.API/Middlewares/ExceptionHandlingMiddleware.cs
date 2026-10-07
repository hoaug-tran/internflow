using InternFlow.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using InternFlow.Domain.Evaluations;
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
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Response đã bắt đầu gửi về client, không thể ghi ProblemDetails.");
                throw;
            }

            var status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                ForbiddenException => StatusCodes.Status403Forbidden,
<<<<<<< HEAD
                EvaluationRuleException { Rule: EvaluationRule.Conflict } => StatusCodes.Status409Conflict,
                EvaluationRuleException => StatusCodes.Status400BadRequest,
=======
>>>>>>> origin/main
                _ => StatusCodes.Status500InternalServerError
            };

            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning("Handled exception ({Status}): {Message}", status, ex.Message);

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
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
                problem,
                (System.Text.Json.JsonSerializerOptions?)null,
                "application/problem+json");
        }
    }
}
