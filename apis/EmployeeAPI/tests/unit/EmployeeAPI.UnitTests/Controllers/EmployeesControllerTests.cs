using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using EmployeeAPI.Controllers;
using EmployeeAPI.Models;
using System.Net;

namespace EmployeeAPI.UnitTests.Controllers
{
    public class EmployeesControllerTests
    {
        private readonly Mock<ILogger<EmployeesController>> _loggerMock;
        private readonly EmployeesController _controller;
        private readonly DefaultHttpContext _httpContext;

        public EmployeesControllerTests()
        {
            _loggerMock = new Mock<ILogger<EmployeesController>>();
            _controller = new EmployeesController(_loggerMock.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public void Health_ReturnsOkResult()
        {
            var result = _controller.Health();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<HealthResponse>(okResult.Value);
            Assert.Equal("ok", value.Status);
            Assert.Equal("EmployeeAPI", value.Service);
            Assert.False(string.IsNullOrWhiteSpace(value.Timestamp));
        }

        [Fact]
        public void Health_LogsInformation()
        {
            _controller.Health();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}

