using OrderAccumulator.Domain.Entities;
using OrderAccumulator.Domain.ValueObject;

namespace OrderTests.OrderAccumulatorTests.Objects
{
    public class OrderTest
    {
        [Fact]
        public void Constructor_WithValidData_CreatesOrder()
        {
            // Arrange
            var symbol = new Symbol("PETR4");
            var side = OrderSideType.Buy;
            var price = new Money(25.50m);
            var quantity = 100;
            var clOrdId = "ORDER123";

            // Act
            var order = new Order(symbol, side, price, quantity, clOrdId);

            // Assert
            Assert.Equal("PETR4", order.Symbol.Value);
            Assert.Equal(OrderSideType.Buy, order.Side);
            Assert.Equal(25.50m, order.Price.Value);
            Assert.Equal(100, order.Quantity);
            Assert.Equal("ORDER123", order.ClOrdID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1_000_001)]
        public void Constructor_WithInvalidQuantity_ThrowsException(int invalidQuantity)
        {
            // Arrange
            var symbol = new Symbol("PETR4");
            var price = new Money(25.50m);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new Order(symbol, OrderSideType.Buy, price, invalidQuantity, "ORDER123"));
        }

        [Fact]
        public void TotalValue_ReturnsQuantityTimesPrice()
        {
            // Arrange
            var order = new Order(
                new Symbol("PETR4"),
                OrderSideType.Buy,
                new Money(25.50m),
                100,
                "ORDER123"
            );

            // Act
            var total = order.FinancialValue;

            // Assert
            Assert.Equal(2550m, total);
        }
    }
}