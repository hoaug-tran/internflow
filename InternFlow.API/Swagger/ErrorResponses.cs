using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InternFlow.API.Swagger;

/// Khai báo các mã lỗi mà endpoint có thể trả. Đặt trên class thì áp dụng cho mọi action.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ErrorResponsesAttribute(params int[] codes) : Attribute
{
    public int[] Codes { get; } = codes;
}

/// Đọc [ErrorResponses] và [Authorize], tự thêm response lỗi vào Swagger.
public class ErrorResponsesOperationFilter : IOperationFilter
{
    private static readonly Dictionary<int, string> Descriptions = new()
    {
        [400] = "Dữ liệu không hợp lệ",
        [401] = "Chưa đăng nhập hoặc token hết hạn",
        [403] = "Không có quyền thực hiện",
        [404] = "Không tìm thấy dữ liệu",
        [409] = "Xung đột trạng thái dữ liệu"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var controller = method.DeclaringType!;

        var codes = new SortedSet<int>(
            method.GetCustomAttributes(true).Concat(controller.GetCustomAttributes(true))
                .OfType<ErrorResponsesAttribute>()
                .SelectMany(a => a.Codes));

        var requiresAuth =
            (method.GetCustomAttributes(true).Concat(controller.GetCustomAttributes(true)).OfType<AuthorizeAttribute>().Any())
            && !method.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
        if (requiresAuth) codes.Add(401);

        var problemSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

        foreach (var code in codes)
        {
            var key = code.ToString();
            if (operation.Responses.ContainsKey(key)) continue;

            operation.Responses[key] = new OpenApiResponse
            {
                Description = Descriptions.GetValueOrDefault(code, "Lỗi"),
                Content = { ["application/problem+json"] = new OpenApiMediaType { Schema = problemSchema } }
            };
        }
    }
}
