using OrderGenerator.Application.Dto;

namespace OrderGenerator.Application.Interfaces
{
    public interface ISendOrder
    {
        public Task<OrderResult> SendOrderAsync(OrderDto dto);
    }
}
