using OrderAccumulator.Domain.Entities;

namespace OrderAccumulator.Domain.Interface
{
    public interface IExposureCalculator
    {
        decimal CalculateNewExposure(string symbol, Order newOrder);
        bool VerifyExceedLimit(string symbol, Order newOrder);
        void AddOrder(Order order);
        decimal GetCurrentExposure(string symbol);
    }
}
