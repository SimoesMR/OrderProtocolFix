using OrderAccumulator.Domain.ValueObject;

namespace OrderTests.OrderAccumulatorTests.Objects
{
    public class SymbolTest
    {
        [Theory]
        [InlineData("PETR4")]
        [InlineData("VALE3")]
        [InlineData("VIIA4")]
        public void Constructor_WithValidSymbol_CreatesInstance(string validSymbol)
        {
            // Act
            var symbol = new Symbol(validSymbol);

            // Assert
            Assert.Equal(validSymbol, symbol.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("INVALID")]
        [InlineData("BBDC4")]
        public void Constructor_WithInvalidSymbol_ThrowsException(string invalidSymbol)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Symbol(invalidSymbol));
        }
    }
}