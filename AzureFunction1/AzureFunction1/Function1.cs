using AzureCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Json;

namespace AzureFunction1
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("SendMail")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req, ILogger log)
        {
            EmailRequest data = new();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(requestBody))
            {
               data = JsonSerializer.Deserialize<EmailRequest>(requestBody) ?? data;
            }
            

            if (data == null || string.IsNullOrEmpty(data.ToEmail) || string.IsNullOrEmpty(data.Subject) || string.IsNullOrEmpty(data.Body))
            {
                return new BadRequestObjectResult("Please provide ToEmail, Subject, and Body in the request body.");
            }

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("aditya.ar139@gmail.com", "Aditya Rana");
            var to = new EmailAddress(data.ToEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, data.Subject, data.Body, data.Body);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return new OkObjectResult("Email sent successfully.");
            }
            else
            {
                return new StatusCodeResult(500);
            }
        }
    }

}