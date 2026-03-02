using Microsoft.AspNetCore.Mvc;
using GupyIntegration.Models.WebhookPayloads;
using System.Net;
using GupyIntegration.Services.Interfaces;
using Newtonsoft.Json;
using GupyIntegration.Attributes;

namespace GupyIntegration.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    [Produces("application/json")]
    [ApiKey]
    [Tags("webhook")]
    public class WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger) : ControllerBase
    {
        private readonly IWebhookService _webhookService = webhookService;
        private readonly ILogger<WebhookController> _logger = logger;

        /// <summary>
        /// Processes job applications moved through Gupy
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/webhook/application-moved
        ///     {
        ///         "action": "application.assigned",
        ///         "data": {
        ///             "jobId": 8451877,
        ///             "applicationId": 529925562
        ///         }
        ///     }
        /// </remarks>
        /// <response code="200">Application processed successfully</response>
        /// <response code="400">Invalid payload format</response>
        /// <response code="401">Unauthorized access</response>
        /// <response code="500">Internal server error during processing</response>
        [HttpPost("application-moved")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HandleApplicationMoved()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("📌 JSON Recebido: " + body);
            var payload = JsonConvert.DeserializeObject<ApplicationMovedPayload>(body);
            if (payload != null)
            {
                _logger.LogInformation("Application moved webhook received");
                _logger.LogInformation("Payload: {Payload}", payload);
                await _webhookService.HandleApplicationMovedAsync(payload);
                return Ok();
            }
            else
            {
                return BadRequest("Invalid payload format");
            }
        }
        /// <summary>
        /// Processes new job postings published on Gupy
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/webhook/job-published
        ///     {
        ///         "action": "job.published",
        ///         "data": {
        ///             "jobId": 8451877,
        ///             "title": "Teste de Integração"
        ///         }
        ///     }
        /// </remarks>
        /// <response code="200">Job processed and stored successfully</response>
        /// <response code="400">Invalid payload format or missing required data</response>
        /// <response code="401">Unauthorized access - invalid API key</response>
        /// <response code="500">Internal server error during processing</response>
        [HttpPost("job-published")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> HandleJobPublished()
        {
            using var reader = new StreamReader(Request.Body);
            var token = Request.Headers["X-Api-Key"].ToString();
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("📌 JSON Recebido: " + body);
            var payload = JsonConvert.DeserializeObject<JobPublishedPayload>(body);
            if (payload == null)
            {
                return BadRequest("Invalid payload format");
            }
            _logger.LogInformation("Job published webhook received");
            _logger.LogInformation("Payload: {Payload}", payload);
            await _webhookService.HandleJobPublishedAsync(payload);
            return Ok();
        }
    }
}