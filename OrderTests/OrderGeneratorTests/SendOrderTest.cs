using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Interfaces;
using OrderGenerator.Application.Service;

namespace OrderTests.OrderGeneratorTests
{
    public class SendOrderTest
    {
        private readonly Mock<IFixOrderSender> _mockFixOrderSender;
        private readonly Mock<IValidator<OrderDto>> _mockValidator;
        private readonly SendOrder _sendOrder;

        public SendOrderTest()
        {
            _mockFixOrderSender = new Mock<IFixOrderSender>();
            _mockValidator = new Mock<IValidator<OrderDto>>();
            _sendOrder = new SendOrder(_mockFixOrderSender.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task SendOrderAsync_WithValidOrder_ReturnsSuccessResult()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, 25.50m);
            var validationResult = new ValidationResult();  // Sem erros = válido
            var expectedResult = new OrderResult(true, "Ordem enviada com sucesso");

            _mockValidator
                .Setup(x => x.ValidateAsync(orderDto, default))
                .ReturnsAsync(validationResult);

            _mockFixOrderSender
                .Setup(x => x.FixOrderSenderAsync(orderDto))
                .ReturnsAsync(expectedResult);

            var result = await _sendOrder.SendOrderAsync(orderDto);

            Assert.True(result.Accepted);
            Assert.Equal("Ordem enviada com sucesso", result.Message);
            _mockValidator.Verify(x => x.ValidateAsync(orderDto, default), Times.Once);
            _mockFixOrderSender.Verify(x => x.FixOrderSenderAsync(orderDto), Times.Once);
        }

        [Fact]
        public async Task SendOrderAsync_WithInvalidOrder_ReturnsFailureResult()
        {
            var orderDto = new OrderDto("", "BUY", 100, 25.50m);
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Symbol", "Symbol é obrigatório")
            });

            _mockValidator
                .Setup(x => x.ValidateAsync(orderDto, default))
                .ReturnsAsync(validationResult);

            var result = await _sendOrder.SendOrderAsync(orderDto);

            Assert.False(result.Accepted);
            Assert.Equal("Symbol é obrigatório", result.Message);
            _mockFixOrderSender.Verify(x => x.FixOrderSenderAsync(It.IsAny<OrderDto>()), Times.Never);
        }

        [Fact]
        public async Task SendOrderAsync_WithMultipleValidationErrors_ReturnsJoinedErrorMessages()
        {
            var orderDto = new OrderDto("", "", 0, 0m);
            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Symbol", "Symbol é obrigatório"),
                new ValidationFailure("Side", "Side é obrigatório"),
                new ValidationFailure("Quantity", "Quantity deve ser maior que zero")
            });

            _mockValidator
                .Setup(x => x.ValidateAsync(orderDto, default))
                .ReturnsAsync(validationResult);

            var result = await _sendOrder.SendOrderAsync(orderDto);

            Assert.False(result.Accepted);
            Assert.Contains("Symbol é obrigatório", result.Message);
            Assert.Contains("Side é obrigatório", result.Message);
            Assert.Contains("Quantity deve ser maior que zero", result.Message);
            Assert.Contains("; ", result.Message);  // Verifica que usou o separador
        }

        [Fact]
        public async Task SendOrderAsync_WhenFixOrderSenderFails_ReturnsFailureResult()
        {
            var orderDto = new OrderDto("VALE3", "SELL", 50, 68.00m);
            var validationResult = new ValidationResult();
            var failureResult = new OrderResult(false, "Falha ao conectar com a corretora");

            _mockValidator
                .Setup(x => x.ValidateAsync(orderDto, default))
                .ReturnsAsync(validationResult);

            _mockFixOrderSender
                .Setup(x => x.FixOrderSenderAsync(orderDto))
                .ReturnsAsync(failureResult);

            var result = await _sendOrder.SendOrderAsync(orderDto);

            Assert.False(result.Accepted);
            Assert.Equal("Falha ao conectar com a corretora", result.Message);
        }

        [Fact]
        public async Task SendOrderAsync_ValidOrder_CallsFixOrderSenderOnlyAfterValidation()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, 25.50m);
            var validationResult = new ValidationResult();
            var expectedResult = new OrderResult(true, "Sucesso");
            var callOrder = new List<string>();

            _mockValidator
                .Setup(x => x.ValidateAsync(orderDto, default))
                .Callback(() => callOrder.Add("Validator"))
                .ReturnsAsync(validationResult);

            _mockFixOrderSender
                .Setup(x => x.FixOrderSenderAsync(orderDto))
                .Callback(() => callOrder.Add("FixOrderSender"))
                .ReturnsAsync(expectedResult);

            await _sendOrder.SendOrderAsync(orderDto);

            Assert.Equal(2, callOrder.Count);
            Assert.Equal("Validator", callOrder[0]);
            Assert.Equal("FixOrderSender", callOrder[1]);
        }
    }
}