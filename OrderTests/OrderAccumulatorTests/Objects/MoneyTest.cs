using OrderAccumulator.Domain.ValueObject;

namespace OrderTests.OrderAccumulatorTests.Objects
{
    public class MoneyTest
    {
        [Fact]
        public void Constructor_WithPositiveValue_CreatesInstance()
        {
            // Act
            var money = new Money(100.50m);

            // Assert
            Assert.Equal(100.50m, money.Value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-0.01)]
        public void Constructor_WithNegativeValue_ThrowsException(decimal invalidValue)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Money(invalidValue));
        }
    }
}