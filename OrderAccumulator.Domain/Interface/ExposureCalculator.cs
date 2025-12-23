using OrderAccumulator.Domain.Entities;
using OrderAccumulator.Domain.ValueObject;

namespace OrderAccumulator.Domain.Interface
{
    public class ExposureCalculator : IExposureCalculator
    {
        private const decimal EXPOSURE_LIMIT = 100_000_000; 
        private readonly Dictionary<string, SymbolExposure> _exposures = new();

        public decimal CalculateNewExposure(string symbol, Order newOrder)
        {
            var currentExposure = GetCurrentExposure(symbol);
            var orderValue = newOrder.FinancialValue;

            // Buy (1) aumenta exposição, Sell (2) diminui
            return newOrder.Side == OrderSideType.Buy
                ? currentExposure + orderValue
                : currentExposure - orderValue;

        }

        public bool VerifyExceedLimit(string symbol, Order newOrder)
        {
            var projectedExposure = CalculateNewExposure(symbol, newOrder);
            return Math.Abs(projectedExposure) >= EXPOSURE_LIMIT || projectedExposure < 0;
        }

        public void AddOrder(Order order)
        {
            //verify if my dictionary contain the symbol, if not add the symbol in dictionary
            if (!_exposures.ContainsKey(order.Symbol.Value))
            {
                _exposures[order.Symbol.Value] = new SymbolExposure
                {
                    Symbol = order.Symbol.Value
                };
            }

            _exposures[order.Symbol.Value].Orders.Add(order);
            _exposures[order.Symbol.Value].CurrentExposure = CalculateNewExposure(order.Symbol.Value, order);
        }

        public decimal GetCurrentExposure(string symbol)
        {
            //verify if my dictionary contain the symbol, if not return 0
            if (!_exposures.ContainsKey(symbol))
                return 0m;

            var exposure = _exposures[symbol];
            decimal total = 0m;

            foreach (var order in exposure.Orders)
            {
                var value = order.FinancialValue;
                //if side is buy sum value if not subtract de value
                total += order.Side == OrderSideType.Buy ? value : -value;
            }

            return total;
        }
    }
}
