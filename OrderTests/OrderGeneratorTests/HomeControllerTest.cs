using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Web.Controllers;
using Microsoft.Extensions.Logging;
using OrderGenerator.Application.Interfaces;
using OrderGenerator.Application.Dto;

namespace OrderTests.OrderGeneratorTests
{
    public class HomeControllerTest
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<ISendOrder> _mockSendOrder;
        private readonly HomeController _controller;

        public HomeControllerTest()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockSendOrder = new Mock<ISendOrder>();
            _controller = new HomeController(_mockLogger.Object, _mockSendOrder.Object);
        }

        [Fact]
        public async Task SendOrder_WithValidOrder_ReturnsOkResult()
        {
            // Arrange
            var orderDto = new OrderDto("PETR4", "BUY", 100, 25.50m);
            var expectedResult = new OrderResult(true, "Order sent successfully");

            _mockSendOrder
                .Setup(x => x.SendOrderAsync(It.IsAny<OrderDto>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.SendOrder(orderDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(expectedResult, okResult.Value);
            _mockSendOrder.Verify(x => x.SendOrderAsync(orderDto), Times.Once);
        }

        [Fact]
        public async Task SendOrder_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDto("", "", 0, 0m);
            _controller.ModelState.AddModelError("Symbol", "Symbol is required");

            // Act
            var result = await _controller.SendOrder(orderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            _mockSendOrder.Verify(x => x.SendOrderAsync(It.IsAny<OrderDto>()), Times.Never);
        }

        [Fact]
        public async Task SendOrder_WithInvalidModel_LogsError()
        {
            // Arrange
            var orderDto = new OrderDto("", "", 0, 0m);
            _controller.ModelState.AddModelError("Symbol", "Symbol is required");

            // Act
            await _controller.SendOrder(orderDto);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ordem inválida")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}