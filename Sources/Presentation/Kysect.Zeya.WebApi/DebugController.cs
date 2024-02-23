using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.WebApi;


public class DebugController : Controller
{
    private readonly ILogger<DebugController> _logger;

    public DebugController(ILogger<DebugController> logger)
    {
        _logger = logger;
    }

    [HttpGet("TestMethod")]
    public void TestMethod()
    {
        _logger.LogInformation("Test method");
    }
}