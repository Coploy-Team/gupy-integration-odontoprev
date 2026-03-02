using Microsoft.AspNetCore.Mvc;

namespace GupyIntegration.Controllers
{
  [ApiController]
  [Route("api/health")]
  [Produces("application/json")]
  [Tags("health")]
  public class HealthController : ControllerBase
  {
    /// <summary>
    /// Verify if the API is running
    /// </summary>
    /// <remarks>
    /// Endpoint sample to verify if the API is running
    /// </remarks>
    /// <response code="200">API is running 🚀</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
      return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") });
    }
  }
}