using GupyIntegration.Models.Email;
using GupyIntegration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GupyIntegration.Controllers
{
    [ApiController]
    [Route("api/email")]
    [Produces("application/json")]
    [Tags("email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Send email to candidate with job posting information
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     POST /api/email/canditate-created
        /// 
        /// {
        ///     "linkVaga": "https://interview.coploy.io/job/123456/company/123456",
        ///     "email": "candidate@example.com",
        ///     "senha": "123456",
        ///     "nomeVaga": "Desenvolvedor Full Stack"
        /// }
        /// The endpoint supports:
        /// * Send email to candidate with job posting information
        /// </remarks>
        /// <param name="request">data to send email</param>
        /// <response code="200">send email success</response>
        /// <response code="500">error to send email</response>
        [HttpPost("canditate-created")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendJobEmail([FromBody] JobEmailRequest request)
        {
            try
            {
                await _emailService.SendJobPostingEmailAsync(
                    request.LinkVaga,
                    request.Email,
                    request.Senha,
                    request.NomeVaga
                );

            return Ok(new { message = "Send email success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to send email");
            return StatusCode(500, new { message = "Error to send email" });
        }
    }
}
}
