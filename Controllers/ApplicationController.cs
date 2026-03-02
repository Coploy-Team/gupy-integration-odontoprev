using GupyIntegration.Attributes;
using GupyIntegration.Models;
using GupyIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GupyIntegration.Controllers
{
  /// <summary>
  /// Controller for managing job applications through Gupy integration
  /// </summary>
  [ApiController]
  [ApiKey]
  [Route("api/application")]
  [Produces("application/json")]
  [Tags("application")]
  public class ApplicationController : ControllerBase
  {
    private readonly IGupyApplicationService _gupyApplicationService;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(IGupyApplicationService gupyApplicationService, ILogger<ApplicationController> logger)
    {
      _gupyApplicationService = gupyApplicationService;
      _logger = logger;
    }

    /// <summary>
    /// Retrieves applications for a specific job
    /// </summary>
    /// <param name="jobId">ID of the job to get applications for</param>
    /// <param name="applicationId">Optional specific application ID to retrieve</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="perPage">Number of items per page (default: 10)</param>
    /// <param name="order">Sorting order (default: "id asc")</param>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/application/123456/applications
    ///     GET /api/application/123456/applications?applicationId=789012
    ///     GET /api/application/123456/applications?page=2&amp;perPage=20&amp;order=id desc
    /// 
    /// Supported ordering options:
    /// * id asc/desc
    /// * created_at asc/desc
    /// * updated_at asc/desc
    /// </remarks>
    /// <returns>List of applications for the specified job</returns>
    /// <response code="200">Returns the list of applications</response>
    /// <response code="401">Unauthorized - Invalid API key</response>
    /// <response code="404">Job or applications not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{jobId}/applications")]
    [ProducesResponseType(typeof(GupyApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GupyApplicationResponse>> GetApplications(
        [FromRoute] long jobId,
        [FromQuery] long? applicationId = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 10,
        [FromQuery] string order = "id asc")
    {
      try
      {
        _logger.LogInformation("Buscando aplicações para o jobId: {JobId}", jobId);
        var result = await _gupyApplicationService.GetApplicationsAsync(jobId, applicationId, page, perPage, order);
        return Ok(result);
      }
      catch (HttpRequestException ex)
      {
        return StatusCode((int)ex.StatusCode!, ex.Message);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
      }
    }

    /// <summary>
    /// Adds a tag to a specific job application
    /// </summary>
    /// <param name="jobId">ID of the job</param>
    /// <param name="applicationId">ID of the application to tag</param>
    /// <param name="tag">Tag information to be added</param>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/application/123456/applications/789012/tags
    ///     {
    ///         "name": "interview_scheduled",
    ///         "color": "#FF0000"
    ///     }
    /// 
    /// The tag object should contain:
    /// * name: String identifying the tag
    /// * color: Hex color code for the tag (optional)
    /// </remarks>
    /// <returns>Success confirmation</returns>
    /// <response code="200">Tag successfully added</response>
    /// <response code="400">Invalid tag data</response>
    /// <response code="401">Unauthorized - Invalid API key</response>
    /// <response code="404">Job or application not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{jobId}/applications/{applicationId}/tags")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTagToApplication(
        [FromRoute] long jobId,
        [FromRoute] long applicationId,
        [FromBody] ApplicationTag tag)
    {
      try
      {
        _logger.LogInformation("Iniciando tag à aplicação - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}", jobId, applicationId, tag.Name);
        await _gupyApplicationService.AddTagToApplicationAsync(jobId, applicationId, tag);
        return Ok(new { success = true });
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError("Erro ao adicionar tag à aplicação - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}, Erro: {Error}", jobId, applicationId, tag, ex.Message);
        return StatusCode((int)ex.StatusCode!, ex.Message);
      }
      catch (Exception ex)
      {
        _logger.LogError("Erro ao adicionar tag à aplicação - JobId: {JobId}, ApplicationId: {ApplicationId}, Tag: {Tag}, Erro: {Error}", jobId, applicationId, tag, ex.Message);
        return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
      }
    }
  }
}