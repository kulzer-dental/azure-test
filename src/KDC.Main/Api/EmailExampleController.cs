using KDC.Main.Services;
using Microsoft.AspNetCore.Mvc;

namespace KDC.Main.Api
{
    /// <summary>
    /// A demo controller for the api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmailExampleController() : ControllerBase
    {

        /// <summary>
        /// Sends a single mail
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> SendSingleEmail([FromServices] IEmailService sender)
        {
            var template = "Dear {{Name}}, You are totally {{Compliment}}.";

            await sender.SendEmailAsync("test@test.test", "test email", template, new { Name = "Luke", Compliment = "Awesome" });

            return Ok();
        }
    }

    public class EmailExample
    {
        private readonly IEmailService _emailService;

        public EmailExample(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendSingleEmail()
        {
            var template = "Dear {{Name}}, You are totally {{Compliment}}.";

            await _emailService.SendEmailAsync("test@test.test", "test email", template, new { Name = "Luke", Compliment = "Awesome" });
        }
    }
}
