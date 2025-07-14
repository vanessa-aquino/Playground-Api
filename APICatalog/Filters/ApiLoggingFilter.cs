using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalog.Filters;

public class ApiLoggingFilter : IActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Executa antes da Action
        _logger.LogInformation("### Executing -> OnActionExecuting");
        _logger.LogInformation("#####################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"Model State: {context.ModelState.IsValid}");
        _logger.LogInformation("#####################################");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Executa depois da Action
        _logger.LogInformation("### Executed -> OnActionExecuted");
        _logger.LogInformation("#####################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"Status Code: {context.HttpContext.Response.StatusCode}");
        _logger.LogInformation("#####################################");
    }
}
