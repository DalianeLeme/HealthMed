using FluentAssertions;
using HealthMed.Appointments.Application.Clients;
using HealthMed.Shared.DTOs;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace HealthMed.Appointments.Application.Tests.Clients
{
    public class AuthClientTests
    {
        private HttpClient CreateMockedHttpClient(HttpResponseMessage response, Action<HttpRequestMessage>? inspectRequest = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
                {
                    inspectRequest?.Invoke(request);
                    return response;
                });

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ShouldReturnList_WhenResponseIsOk()
        {
            // Arrange
            var expected = new List<UserDto> { new UserDto { Name = "Dr. Who" } };
            var content = new StringContent(JsonSerializer.Serialize(expected));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            var client = new AuthClient(CreateMockedHttpClient(response));

            // Act
            var result = await client.GetAllDoctorsAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Dr. Who");
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ShouldIncludeSpecialtyInUrl_WhenProvided()
        {
            // Arrange
            string calledUrl = "";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };

            var client = new AuthClient(CreateMockedHttpClient(response, r => calledUrl = r.RequestUri!.ToString()));

            // Act
            await client.GetAllDoctorsAsync("Cardiology");

            // Assert
            calledUrl.Should().Contain("?specialty=Cardiology");
        }

        [Fact]
        public async Task GetAllDoctorsAsync_ShouldReturnEmptyList_WhenContentIsEmptyArray()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json") // lista JSON vazia válida
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            var client = new AuthClient(httpClient);

            // Act
            var result = await client.GetAllDoctorsAsync();

            // Assert
            result.Should().BeEmpty();
        }


        [Fact]
        public async Task GetAllDoctorsAsync_ShouldThrow_WhenResponseIsFailure()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var client = new AuthClient(CreateMockedHttpClient(response));

            // Act
            Func<Task> act = async () => await client.GetAllDoctorsAsync();

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
