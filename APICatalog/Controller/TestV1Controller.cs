using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APICatalog.Controller;

[Route("api/v{version:apiVersion}/test")]
[ApiController]
[ApiVersion("1.0", Deprecated = true)] // Informar aos usuarios que a versão está obsoleta e pode ser descontinuada a qualquer momento.
[ApiExplorerSettings(IgnoreApi = true)]
public class TestV1Controller : ControllerBase
{
    [HttpGet]
    public string GetVersion() => "TestV1 - GET - Api Version 1.0";
}
