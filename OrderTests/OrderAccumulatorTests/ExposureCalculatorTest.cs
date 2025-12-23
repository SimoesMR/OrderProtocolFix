using OrderAccumulator.Domain.Entities;
using OrderAccumulator.Domain.Interface;
using OrderAccumulator.Domain.ValueObject;
using System.Runtime.Intrinsics.Arm;

namespace OrderTests.OrderAccumulatorTests
{
    public class ExposureCalculatorTest
    {
        private readonly ExposureCalculator _calculator;

        public ExposureCalculatorTest()
        {
            _calculator = new ExposureCalculator();
        }

        #region GetCurrentExposure Tests

        [Fact]
        public void GetCurrentExposure_WithNoOrders_ReturnsZero()
        {
            var exposure = _calculator.GetCurrentExposure("PETR4");

            Assert.Equal(0m, exposure);
        }

        [Fact]
        public void GetCurrentExposure_WithBuyOrder_ReturnsPositiveValue()
        {
            var order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            _calculator.AddOrder(order);

            var exposure = _calculator.GetCurrentExposure("PETR4");

            Assert.Equal(2500m, exposure);
        }

        [Fact]
        public void GetCurrentExposure_WithSellOrder_ReturnsNegativeValue()
        {
            var order = CreateOrder("PETR4", OrderSideType.Sell, 100, 25.00m);
            _calculator.AddOrder(order);

            var exposure = _calculator.GetCurrentExposure("PETR4");

            Assert.Equal(-2500m, exposure);
        }

        [Fact]
        public void GetCurrentExposure_WithMultipleBuyOrders_ReturnsSumOfValues()
        {
            var order1 = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            var order2 = CreateOrder("PETR4", OrderSideType.Buy, 50, 30.00m);
            _calculator.AddOrder(order1);
            _calculator.AddOrder(order2);

            var exposure = _calculator.GetCurrentExposure("PETR4");

            Assert.Equal(4000m, exposure); 
        }

        [Fact]
        public void GetCurrentExposure_WithBuyAndSellOrders_ReturnsNetExposure()
        {
            var buyOrder = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            var sellOrder = CreateOrder("PETR4", OrderSideType.Sell, 50, 25.00m);
            _calculator.AddOrder(buyOrder);
            _calculator.AddOrder(sellOrder);

            var exposure = _calculator.GetCurrentExposure("PETR4");

            Assert.Equal(1250m, exposure);
        }

        [Fact]
        public void GetCurrentExposure_WithDifferentSymbols_ReturnsCorrectExposurePerSymbol()
        {
            var petr4Order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            var vale3Order = CreateOrder("VALE3", OrderSideType.Buy, 50, 68.00m);
            _calculator.AddOrder(petr4Order);
            _calculator.AddOrder(vale3Order);

            var petr4Exposure = _calculator.GetCurrentExposure("PETR4");
            var vale3Exposure = _calculator.GetCurrentExposure("VALE3");

            Assert.Equal(2500m, petr4Exposure);
            Assert.Equal(3400m, vale3Exposure);
        }

        #endregion

        #region CalculateNewExposure Tests

        [Fact]
        public void CalculateNewExposure_WithNoPreviousOrders_ReturnsOrderValue()
        {
            var order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);

            var newExposure = _calculator.CalculateNewExposure("PETR4", order);

            Assert.Equal(2500m, newExposure);
        }

        [Fact]
        public void CalculateNewExposure_BuyOrderAddsToExposure()
        {
            var existingOrder = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            _calculator.AddOrder(existingOrder);

            var newOrder = CreateOrder("PETR4", OrderSideType.Buy, 50, 30.00m);

            var newExposure = _calculator.CalculateNewExposure("PETR4", newOrder);

            Assert.Equal(4000m, newExposure);
        }

        [Fact]
        public void CalculateNewExposure_SellOrderSubtractsFromExposure()
        {
            var existingOrder = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            _calculator.AddOrder(existingOrder);

            var newOrder = CreateOrder("PETR4", OrderSideType.Sell, 50, 25.00m);

            var newExposure = _calculator.CalculateNewExposure("PETR4", newOrder);

            Assert.Equal(1250m, newExposure);
        }

        #endregion

        #region VerifyExceedLimit Tests

        [Fact]
        public void VerifyExceedLimit_WhenBelowLimit_ReturnsFalse()
        {
            var order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", order);

            Assert.False(exceeds);
        }

        [Fact]
        public void VerifyExceedLimit_WhenAtLimit_ReturnsTrue()
        {
            var order1 = CreateOrder("PETR4", OrderSideType.Buy, 99_999, 999.99m);
            _calculator.AddOrder(order1);

            var order2 = CreateOrder("PETR4", OrderSideType.Buy, 99_999, 1.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", order2);
            
            Assert.True(exceeds);
        }

        [Fact]
        public void VerifyExceedLimit_WhenAboveLimit_ReturnsTrue()
        {
            var order1 = CreateOrder("PETR4", OrderSideType.Buy, 50_000, 999.00m);
            _calculator.AddOrder(order1);

            var order2 = CreateOrder("PETR4", OrderSideType.Buy, 50_000, 999.00m);
            _calculator.AddOrder(order2);

            var newOrder = CreateOrder("PETR4", OrderSideType.Buy, 500, 250.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", newOrder);

            // Assert
            Assert.True(exceeds);
        }

        [Fact]
        public void VerifyExceedLimit_WhenExposureWouldGoNegative_ReturnsTrue()
        {
           var order = CreateOrder("PETR4", OrderSideType.Sell, 100, 25.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", order);

            Assert.True(exceeds);
        }

        [Fact]
        public void VerifyExceedLimit_WhenCumulativeExceedsLimit_ReturnsTrue()
        {
            var existingOrder = CreateOrder("PETR4", OrderSideType.Buy, 99999, 999.00m);
            _calculator.AddOrder(existingOrder);

            var newOrder = CreateOrder("PETR4", OrderSideType.Buy, 99999, 999.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", newOrder);

            Assert.True(exceeds);
        }

        [Fact]
        public void VerifyExceedLimit_WhenCumulativeBelowLimit_ReturnsFalse()
        {
            var existingOrder = CreateOrder("PETR4", OrderSideType.Buy, 200, 25.00m);
            _calculator.AddOrder(existingOrder);

            var newOrder = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);


            var exceeds = _calculator.VerifyExceedLimit("PETR4", newOrder);

            Assert.False(exceeds);
        }

        #endregion

        #region AddOrder Tests

        [Fact]
        public void AddOrder_WithNewSymbol_CreatesExposureEntry()
        {
            var order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);

            _calculator.AddOrder(order);

            var exposure = _calculator.GetCurrentExposure("PETR4");
            Assert.Equal(2500m, exposure);
        }

        [Fact]
        public void AddOrder_WithExistingSymbol_UpdatesExposure()
        {

            var order1 = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            var order2 = CreateOrder("PETR4", OrderSideType.Buy, 50, 25.00m);


            _calculator.AddOrder(order1);
            _calculator.AddOrder(order2);

            var exposure = _calculator.GetCurrentExposure("PETR4");
            Assert.Equal(3750m, exposure);
        }

        [Fact]
        public void AddOrder_MultipleSymbols_TracksIndependently()
        {

            var petr4Order = CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m);
            var vale3Order = CreateOrder("VALE3", OrderSideType.Buy, 100, 68.00m);
            var viia4Order = CreateOrder("VIIA4", OrderSideType.Buy, 100, 10.00m);


            _calculator.AddOrder(petr4Order);
            _calculator.AddOrder(vale3Order);
            _calculator.AddOrder(viia4Order);

            Assert.Equal(2500m, _calculator.GetCurrentExposure("PETR4"));
            Assert.Equal(6800m, _calculator.GetCurrentExposure("VALE3"));
            Assert.Equal(1000m, _calculator.GetCurrentExposure("VIIA4"));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void FullWorkflow_BuyAndSellOrders_CalculatesCorrectExposure()
        {
            _calculator.AddOrder(CreateOrder("PETR4", OrderSideType.Buy, 100, 25.00m));
            Assert.Equal(2500m, _calculator.GetCurrentExposure("PETR4"));

            _calculator.AddOrder(CreateOrder("PETR4", OrderSideType.Buy, 50, 26.00m));
            Assert.Equal(3800m, _calculator.GetCurrentExposure("PETR4"));

            _calculator.AddOrder(CreateOrder("PETR4", OrderSideType.Sell, 30, 27.00m));
            Assert.Equal(2990m, _calculator.GetCurrentExposure("PETR4"));
        }

        [Fact]
        public void FullWorkflow_VerifyLimitBeforeAdd_PreventsExcessiveExposure()
        {
            //Adiciiona ordem R$ 49.950.000
            var order1 = CreateOrder("PETR4", OrderSideType.Buy, 50_000, 999.00m);
            if (!_calculator.VerifyExceedLimit("PETR4", order1))
            {
                _calculator.AddOrder(order1);
            }

            //Adiciiona ordem R$ 49.950.000
            var order2 = CreateOrder("PETR4", OrderSideType.Buy, 50_000, 999.00m);
            if (!_calculator.VerifyExceedLimit("PETR4", order2))
            {
                _calculator.AddOrder(order2);
            }

            // Tenta adicionar ordem que excederia o limite R$ 99.900.000 + R$ 150.000 = R$ 100.050.000
            var order3 = CreateOrder("PETR4", OrderSideType.Buy, 1_000, 150.00m);

            var exceeds = _calculator.VerifyExceedLimit("PETR4", order3);

            Assert.True(exceeds);  // Deve exceder
            Assert.Equal(99_900_000m, _calculator.GetCurrentExposure("PETR4"));
        }

        #endregion

        #region Helper Methods

        private Order CreateOrder(string symbol, OrderSideType side, int quantity, decimal price)
        {
            return new Order(
                new Symbol(symbol),
                side,
                new Money(price),
                quantity,
                Guid.NewGuid().ToString()
            );
        }

        #endregion
    }
}