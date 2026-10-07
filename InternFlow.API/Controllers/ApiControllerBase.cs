using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers.Conventions;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;
    protected ISender Sender =>
        _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
