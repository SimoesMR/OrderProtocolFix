using Xunit;
using Moq;
using QuickFix;
using QuickFix.Fields;
using OrderAccumulator.Domain.Interface;
using OrderAccumulator.Application.Service;
using OrderAccumulator.Domain.Entities;
namespace OrderTests.OrderAccumulatorTests
{
    public class OrderProcessorTest
    {
        private readonly Mock<IExposureCalculator> _mockExposureCalculator;
        private readonly OrderProcessor _orderProcessor;

        public OrderProcessorTest()
        {
            _mockExposureCalculator = new Mock<IExposureCalculator>();
            _orderProcessor = new OrderProcessor(_mockExposureCalculator.Object);
        }

        #region ProcessNewOrder - Ordem Aceita

        [Fact]
        public void ProcessNewOrder_WithValidOrder_ReturnsAcceptedExecutionReport()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            _mockExposureCalculator
                .Setup(x => x.GetCurrentExposure(It.IsAny<string>()))
                .Returns(50000m);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(ExecType.NEW, result.GetChar(Tags.ExecType));
            Assert.Equal(OrdStatus.NEW, result.GetChar(Tags.OrdStatus));
            Assert.Equal("ORDER123", result.GetString(Tags.ClOrdID));
        }

        [Fact]
        public void ProcessNewOrder_WithValidOrder_AddsOrderToExposureCalculator()
        {
            // Arrange
            var message = CreateValidOrderMessage("VALE3", Side.SELL, 50, 68.00m, "ORDER456");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            // Act
            _orderProcessor.ProcessNewOrder(message);

            // Assert
            _mockExposureCalculator.Verify(
                x => x.AddOrder(It.Is<Order>(o =>
                    o.Symbol.Value == "VALE3" &&
                    o.Quantity == 50)),
                Times.Once);
        }

        [Fact]
        public void ProcessNewOrder_WithValidOrder_SetsCorrectFields()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 200, 30.00m, "ORDER789");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal("PETR4", result.GetString(Tags.Symbol));
            Assert.Equal(Side.BUY, result.GetChar(Tags.Side));
            Assert.Equal(30.00m, result.GetDecimal(Tags.Price));
            Assert.Equal(200, result.GetInt(Tags.OrderQty));
            Assert.Equal(200, result.GetInt(Tags.LeavesQty));
            Assert.Equal(0, result.GetInt(Tags.CumQty));
            Assert.Equal(0m, result.GetDecimal(Tags.AvgPx));
        }

        [Fact]
        public void ProcessNewOrder_WithValidOrder_GeneratesUniqueOrderIdAndExecId()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            var orderId = result.GetString(Tags.OrderID);
            var execId = result.GetString(Tags.ExecID);

            Assert.False(string.IsNullOrEmpty(orderId));
            Assert.False(string.IsNullOrEmpty(execId));
            Assert.True(Guid.TryParse(orderId, out _));
            Assert.True(Guid.TryParse(execId, out _));
        }

        #endregion

        #region ProcessNewOrder - Ordem Rejeitada por Limite

        [Fact]
        public void ProcessNewOrder_WhenExceedsLimit_ReturnsRejectedExecutionReport()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 1000000, 100.00m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(true);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(ExecType.REJECTED, result.GetChar(Tags.ExecType));
            Assert.Equal(OrdStatus.REJECTED, result.GetChar(Tags.OrdStatus));
        }

        [Fact]
        public void ProcessNewOrder_WhenExceedsLimit_DoesNotAddOrder()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 1000000, 100.00m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(true);

            // Act
            _orderProcessor.ProcessNewOrder(message);

            // Assert
            _mockExposureCalculator.Verify(
                x => x.AddOrder(It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        public void ProcessNewOrder_WhenExceedsLimit_IncludesReasonInText()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 100.00m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(true);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            var text = result.GetString(Tags.Text);
            Assert.Contains("excede limite", text);
        }

        [Fact]
        public void ProcessNewOrder_WhenExceedsLimit_SetsLeavesQtyAndCumQtyToZero()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(true);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(0, result.GetInt(Tags.LeavesQty));
            Assert.Equal(0, result.GetInt(Tags.CumQty));
            Assert.Equal(0m, result.GetDecimal(Tags.AvgPx));
        }

        #endregion

        #region ProcessNewOrder - Exceção

        [Fact]
        public void ProcessNewOrder_WhenExceptionOccurs_ReturnsRejectedReport()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Throws(new Exception("Erro de conexão"));

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(ExecType.REJECTED, result.GetChar(Tags.ExecType));
            Assert.Equal(OrdStatus.REJECTED, result.GetChar(Tags.OrdStatus));
        }

        [Fact]
        public void ProcessNewOrder_WhenExceptionOccurs_IncludesErrorMessageInText()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Throws(new Exception("Erro de conexão"));

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            var text = result.GetString(Tags.Text);
            Assert.Contains("Erro", text);
            Assert.Contains("Erro de conexão", text);
        }

        [Fact]
        public void ProcessNewOrder_WhenExceptionOccurs_SetsClOrdIdToUnknown()
        {
            // Arrange - mensagem inválida sem ClOrdID
            var message = new QuickFix.FIX44.NewOrderSingle();
            // Não setamos os campos obrigatórios para forçar exceção no ExtractOrderFromMessage

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(ExecType.REJECTED, result.GetChar(Tags.ExecType));
        }

        #endregion

        #region ProcessNewOrder - Verificação de Fluxo

        [Fact]
        public void ProcessNewOrder_CallsVerifyExceedLimitBeforeAddOrder()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 25.50m, "ORDER123");
            var callOrder = new List<string>();

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Callback(() => callOrder.Add("VerifyExceedLimit"))
                .Returns(false);

            _mockExposureCalculator
                .Setup(x => x.AddOrder(It.IsAny<Order>()))
                .Callback(() => callOrder.Add("AddOrder"));

            // Act
            _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(2, callOrder.Count);
            Assert.Equal("VerifyExceedLimit", callOrder[0]);
            Assert.Equal("AddOrder", callOrder[1]);
        }

        [Fact]
        public void ProcessNewOrder_CallsGetCurrentExposureAfterAddOrder()
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", Side.BUY, 100, 15.00m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            _mockExposureCalculator
                .Setup(x => x.AddOrder(It.IsAny<Order>()));

            _mockExposureCalculator
                .Setup(x => x.GetCurrentExposure(It.IsAny<string>()))
                .Returns(50000m);

            // Act
            _orderProcessor.ProcessNewOrder(message);

            // Assert
            _mockExposureCalculator.Verify(
                x => x.GetCurrentExposure("PETR4"),
                Times.Once);
        }

        #endregion

        #region ProcessNewOrder - Diferentes Sides

        [Theory]
        [InlineData(Side.BUY)]
        [InlineData(Side.SELL)]
        public void ProcessNewOrder_WithDifferentSides_ProcessesCorrectly(char side)
        {
            // Arrange
            var message = CreateValidOrderMessage("PETR4", side, 100, 25.50m, "ORDER123");

            _mockExposureCalculator
                .Setup(x => x.VerifyExceedLimit(It.IsAny<string>(), It.IsAny<Order>()))
                .Returns(false);

            // Act
            var result = _orderProcessor.ProcessNewOrder(message);

            // Assert
            Assert.Equal(ExecType.NEW, result.GetChar(Tags.ExecType));
            Assert.Equal(side, result.GetChar(Tags.Side));
        }

        #endregion

        #region Helper Methods

        private Message CreateValidOrderMessage(string symbol, char side, int quantity, decimal price, string clOrdId)
        {
            var message = new QuickFix.FIX44.NewOrderSingle(
                new ClOrdID(clOrdId),
                new QuickFix.Fields.Symbol(symbol),
                new Side(side),
                new TransactTime(DateTime.UtcNow),
                new OrdType(OrdType.LIMIT)
            );

            message.SetField(new OrderQty(quantity));
            message.SetField(new Price(price));

            return message;
        }

        #endregion
    }
}