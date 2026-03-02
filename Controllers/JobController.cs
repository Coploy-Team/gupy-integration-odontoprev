using GupyIntegration.Models;
using GupyIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using GupyIntegration.Attributes;

namespace GupyIntegration.Controllers
{


  /// <summary>
  /// Controller for managing job-related operations with Gupy integration
  /// </summary>
  [ApiController]
  [ApiKey]
  [Route("api/job")]
  [Produces("application/json")]
  [Tags("job")]
  public class JobController : ControllerBase
  {
    private readonly IJobService _jobService;
    private readonly IWebhookService _webhookService;
    private readonly ILogger<JobController> _logger;

    public JobController(IJobService jobService, IWebhookService webhookService, ILogger<JobController> logger)
    {
      _jobService = jobService;
      _webhookService = webhookService;
      _logger = logger;
    }

    /// <summary>
    /// Retrieves job listings from Gupy with optional filtering
    /// </summary>
    /// <param name="id">Optional specific job ID to retrieve</param>
    /// <param name="perPage">Number of items per page (default: 10)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <remarks>
    /// Sample request:
    ///     GET /api/job?id=123456&amp;perPage=20&amp;page=1
    /// 
    /// The endpoint supports:
    /// * Pagination
    /// * Filtering by specific job ID
    /// * Customizable items per page
    /// </remarks>
    /// <returns>A list of jobs matching the specified criteria</returns>
    /// <response code="200">Returns the list of jobs</response>
    /// <response code="401">Unauthorized - Invalid API key</response>
    /// <response code="404">No jobs found for the specified criteria</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JobResponse>> GetJobs([FromQuery] long? id = null,
                                                       [FromQuery] int perPage = 10,
                                                       [FromQuery] int page = 1)
    {
      try
      {
        var jobs = await _jobService.GetJobsAsync(id, perPage, page);
        return Ok(jobs);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar jobs");
        return StatusCode(500, "Erro interno ao processar a requisição");
      }
    }

    /// <summary>
    /// Processa manualmente uma vaga pelo ID da Gupy (mesmo fluxo do webhook job-published)
    /// </summary>
    /// <param name="jobId">ID da vaga na Gupy</param>
    /// <remarks>
    /// Sample request:
    ///     POST /api/job/process/8451877
    /// 
    /// Executa o mesmo fluxo do webhook job-published:
    /// * Busca a vaga na API da Gupy
    /// * Valida o campo "Haverá teste Técnico? (Coploy)"
    /// * Gera skills e questões via Dify
    /// * Salva a vaga no Firebase
    /// </remarks>
    /// <response code="200">Vaga processada e salva com sucesso</response>
    /// <response code="400">Vaga não encontrada ou não possui teste técnico habilitado</response>
    /// <response code="401">Unauthorized - Invalid API key</response>
    /// <response code="500">Erro interno ao processar a vaga</response>
    [HttpPost("process/{jobId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessJob(long jobId)
    {
      try
      {
        await _webhookService.ProcessJobByIdAsync(jobId);
        return Ok(new { message = $"Vaga {jobId} processada com sucesso" });
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogWarning(ex, "Erro de validação ao processar vaga {JobId}", jobId);
        return BadRequest(new { message = ex.Message });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao processar vaga {JobId}", jobId);
        return StatusCode(500, new { message = "Erro interno ao processar a vaga" });
      }
    }
  }
}