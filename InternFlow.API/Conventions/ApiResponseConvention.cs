using InternFlow.API.Controllers.Conventions;
using InternFlow.Shared.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace InternFlow.API.Conventions;

public sealed class ApiResponseConvention : IControllerModelConvention
{
    private static readonly int[] ErrorStatusCodes =
    {
        StatusCodes.Status400BadRequest,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status409Conflict,
        StatusCodes.Status500InternalServerError
    };

    public void Apply(ControllerModel controller)
    {
        if (!typeof(ApiControllerBase).IsAssignableFrom(
                controller.ControllerType.AsType()))
            return;

        foreach (var action in controller.Actions)
            ApplyToAction(action);
    }

    private static void ApplyToAction(ActionModel action)
    {
        var dataType = ExtractDataFromReturnType(action.ActionMethod.ReturnType);

        action.Filters.Add(new ProducesResponseTypeAttribute(
            typeof(ApiResponse<>).MakeGenericType(dataType),
            StatusCodes.Status200OK));

        foreach (var status in ErrorStatusCodes)
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(
                typeof(ApiResponse<object>),
                status));
        }
    }

    private static Type ExtractDataFromReturnType(Type returnType)
    {
        if (returnType.IsGenericType)
        {
            var genericDef = returnType.GetGenericTypeDefinition();
            if (genericDef == typeof(Task<>) || genericDef == typeof(ValueTask<>))
                returnType = returnType.GetGenericArguments()[0];
        }

        if (returnType.IsGenericType &&
            returnType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            return returnType.GetGenericArguments()[0];

        return typeof(object);
    }
}
