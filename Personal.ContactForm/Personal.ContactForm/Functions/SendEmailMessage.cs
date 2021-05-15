using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MyHealth.Common;
using Personal.ContactForm.Models;
using Microsoft.Extensions.Configuration;

namespace Personal.ContactForm.Functions
{
    public class SendEmailMessage
    {
        private readonly IServiceBusHelpers _serviceBusHelpers;
        private readonly IConfiguration _configuration; 

        public SendEmailMessage(
            IServiceBusHelpers serviceBusHelpers)
        {
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(SendEmailMessage))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Contact")] HttpRequest req,
            ILogger log)
        {
            IActionResult result;

            try
            {
                string messageRequest = await new StreamReader(req.Body).ReadToEndAsync();

                var message = JsonConvert.DeserializeObject<EmailMessage>(messageRequest);

                await _serviceBusHelpers.SendMessageToQueue(_configuration["PersonalSiteEmailQueue"], message);

                result = new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError($"Internal Server Error. Exception thrown: {ex.Message}");
                result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return result;
        }
    }
}
