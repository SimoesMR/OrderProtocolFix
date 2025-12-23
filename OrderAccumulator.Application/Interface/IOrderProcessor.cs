using QuickFix;

namespace OrderAccumulator.Application.Interface
{
    public interface IOrderProcessor
    {
        Message ProcessNewOrder(Message message);
    }
}
