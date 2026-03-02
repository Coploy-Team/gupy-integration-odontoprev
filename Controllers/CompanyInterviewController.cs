using Microsoft.AspNetCore.Mvc;
using GupyIntegration.Services.Interfaces;
using GupyIntegration.Models.Response;
using GupyIntegration.Attributes;

namespace GupyIntegration.Controllers
{
  [ApiController]
  [Route("api/interviews")]
  [Produces("application/json")]
  [ApiKey]
  [Tags("interviews")]
  public class CompanyInterviewController : ControllerBase
  {
    private readonly ICompanyInterviewService _interviewService;
    private readonly ILogger<CompanyInterviewController> _logger;

    public CompanyInterviewController(
        ICompanyInterviewService interviewService,
        ILogger<CompanyInterviewController> logger)
    {
      _interviewService = interviewService;
      _logger = logger;
    }

    /// <summary>
    /// Retorna todas as entrevistas da empresa
    /// </summary>
    /// <remarks>
    /// Busca todas as entrevistas cadastradas para a empresa padrão
    /// </remarks>
    /// <response code="200">Lista de entrevistas encontrada com sucesso</response>
    /// <response code="401">Acesso não autorizado</response>
    /// <response code="500">Erro interno ao processar a requisição</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyInterviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CompanyInterviewResponse>>> GetInterviews()
    {
      try
      {
        var interviews = await _interviewService.GetCompanyInterviewsAsync();
        return Ok(interviews);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar entrevistas");
        return StatusCode(500, new ProblemDetails
        {
          Status = 500,
          Title = "Erro interno do servidor",
          Detail = "Ocorreu um erro ao buscar as entrevistas"
        });
      }
    }

    /// <summary>
    /// Retorna uma entrevista específica
    /// </summary>
    /// <param name="id">ID da entrevista</param>
    /// <remarks>
    /// Busca uma entrevista específica pelo seu ID
    /// </remarks>
    /// <response code="200">Entrevista encontrada com sucesso</response>
    /// <response code="401">Acesso não autorizado</response>
    /// <response code="404">Entrevista não encontrada</response>
    /// <response code="500">Erro interno ao processar a requisição</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyInterviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CompanyInterviewResponse>> GetInterviewById(string id)
    {
      try
      {
        var interview = await _interviewService.GetCompanyInterviewByIdAsync(id);

        if (interview == null)
        {
          return NotFound(new ProblemDetails
          {
            Status = 404,
            Title = "Entrevista não encontrada",
            Detail = $"Não foi encontrada uma entrevista com o ID {id}"
          });
        }

        return Ok(interview);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao buscar entrevista {InterviewId}", id);
        return StatusCode(500, new ProblemDetails
        {
          Status = 500,
          Title = "Erro interno do servidor",
          Detail = "Ocorreu um erro ao buscar a entrevista"
        });
      }
    }

    // /// <summary>
    // /// Retorna uma lista simplificada das entrevistas
    // /// </summary>
    // /// <remarks>
    // /// Retorna apenas external_id, identifier e score das entrevistas finalizadas nos últimos 9 dias
    // /// </remarks>
    // /// <response code="200">Lista simplificada encontrada com sucesso</response>
    // /// <response code="401">Acesso não autorizado</response>
    // /// <response code="500">Erro interno ao processar a requisição</response>
    // [HttpGet("simplified")]
    // [ProducesResponseType(typeof(List<CompanyInterviewSimplifiedResponse>), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    // public async Task<ActionResult<List<CompanyInterviewSimplifiedResponse>>> GetSimplifiedInterviews()
    // {
    //   try
    //   {
    //     var interviews = await _interviewService.GetSimplifiedInterviewsAsync();
    //     return Ok(interviews);
    //   }
    //   catch (Exception ex)
    //   {
    //     _logger.LogError(ex, "Erro ao buscar lista simplificada de entrevistas");
    //     return StatusCode(500, new ProblemDetails
    //     {
    //       Status = 500,
    //       Title = "Erro interno do servidor",
    //       Detail = "Ocorreu um erro ao buscar a lista simplificada de entrevistas"
    //     });
    //   }
    // }
    [HttpGet("questions")]
    [ProducesResponseType(typeof(List<InterviewQuestionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<InterviewQuestionsResponse>>> GetQuestionsAddicional()
    {
      var questions = await _interviewService.GetQuestionsAddicionalAsync();
      return Ok(questions);
    }
  }
}