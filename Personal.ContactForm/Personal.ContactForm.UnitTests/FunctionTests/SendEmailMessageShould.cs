using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using Newtonsoft.Json;
using Personal.ContactForm.Functions;
using Personal.ContactForm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Personal.ContactForm.UnitTests.FunctionTests
{
    public class SendEmailMessageShould
    {
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;
        private Mock<HttpRequest> _mockHttpRequest;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger> _mockLogger;

        private SendEmailMessage _func;

        public SendEmailMessageShould()
        {
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();

            _func = new SendEmailMessage(_mockServiceBusHelpers.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task SendEmailToPersonalSiteEmailQueue()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                SenderEmail = "TestSenderEmail@email.com",
                SenderName = "Test Sender",
                EmailBody = "This is an email",
                EmailSubject = "Test Subject"
            };

            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(emailMessage));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockServiceBusHelpers.Setup(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<EmailMessage>())).Returns(Task.CompletedTask);

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(OkResult), response.GetType());
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<EmailMessage>()), Times.Once);
        }

        [Fact]
        public async Task Throw500WhenSendingEmailToServiceBusFails()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                SenderEmail = "TestSenderEmail@email.com",
                SenderName = "Test Sender",
                EmailBody = "This is an email",
                EmailSubject = "Test Subject"
            };

            byte[] byteArray = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(emailMessage));
            MemoryStream memoryStream = new MemoryStream(byteArray);
            _mockHttpRequest.Setup(r => r.Body).Returns(memoryStream);

            _mockServiceBusHelpers.Setup(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<EmailMessage>())).ThrowsAsync(It.IsAny<Exception>());

            // Act
            var response = await _func.Run(_mockHttpRequest.Object, _mockLogger.Object);

            // Assert
            Assert.Equal(typeof(StatusCodeResult), response.GetType());
            var responseAsStatusCode = (StatusCodeResult)response;
            Assert.Equal(500, responseAsStatusCode.StatusCode);
        }
    }
}
