using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APICatalog.Controller;

[Route("api/v{version:apiVersion}/test")]
[ApiController]
[ApiVersion("3.0")]
[ApiVersion("4.0")]
public class TestV3Controller : ControllerBase
{
    [MapToApiVersion("3.0")]
    [HttpGet]
    public string GetVersion3() => "TestV3 - GET - Api Version 3.0";

    [MapToApiVersion("4.0")]
    [HttpGet]
    public string GetVersion4() => "TestV4 - GET - Api Version 4.0";

}
