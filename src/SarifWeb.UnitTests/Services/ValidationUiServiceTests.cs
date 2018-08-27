using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using Moq;
using SarifWeb.Models;
using SarifWeb.Services;
using SarifWeb.Utilities;
using Xunit;

namespace SarifWeb.UnitTests.Services
{
    public class ValidationUiServiceTests
    {
        private const string WebSiteBaseAddress = "https://sarifweb.example.com";
        private const string PostedFilesDirectory = @"C:\wwwroot\SarifWeb\PostedFiles";

        // This test demonstrates that when the Validation UI service asks the Validation Web API
        // to validate a file, it deserializes the Web API's JSON response and returns the resulting
        // ValidationResponseModel object.
        [Fact]
        public async Task ValidateFileAsync_ReturnsDeserializedValidationApiResponse()
        {
            // Arrange.

            // The JSON response from the service's invocation of the Validation Web API.
            const string ValidationApiResponseMessage = "Hello from Validation API";
            const string ValidationApiResponse = "{ \"Message\": \"" + ValidationApiResponseMessage + "\" }";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DeleteFile(It.IsAny<string>())).Verifiable();

            // Cause the service's invocation of the Validation Web API to return the desired JSON response.
            var mockHttpClientProxy = new Mock<IHttpClientProxy>();
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(ValidationApiResponse)
            };

            mockHttpClientProxy
                .Setup(x => x.PostAsync(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<HttpContent>()))
                .Returns(Task.FromResult(responseMessage));

            var service = new ValidationUiService(mockFileSystem.Object, mockHttpClientProxy.Object);

            // The file to be posted to the Validation Web API.
            var mockPostedFile = new Mock<HttpPostedFileBase>();
            mockPostedFile.SetupGet(x => x.FileName).Returns("anything");
            mockPostedFile.Setup(x => x.SaveAs(It.IsAny<string>())).Verifiable();

            var mockRequest = new Mock<HttpRequestBase>();

            // Act.
            ValidationResponse response = await service.ValidateFileAsync(mockPostedFile.Object, mockRequest.Object, "PostedFilesPath", WebSiteBaseAddress);

            // Assert.
            response.Message.Should().Be(ValidationApiResponseMessage);
        }
    }
}
