using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace APICatalog.Controller;

[Route("api/v{version:apiVersion}/test")]
[ApiController]
[ApiVersion("2.0")]
public class TestV2Controller : ControllerBase
{
    [HttpGet]
    public string GetVersion() => "TestV2 - GET - Api Version 2.0";
}
