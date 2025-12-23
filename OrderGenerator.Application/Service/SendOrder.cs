using FluentValidation;
using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Interfaces;

namespace OrderGenerator.Application.Service
{
    public class SendOrder : ISendOrder
    {
        private readonly IFixOrderSender _fixOrderSender;
        private readonly IValidator<OrderDto> _validator;

        public SendOrder(IFixOrderSender sender, IValidator<OrderDto> validator)
        {
            _fixOrderSender = sender;
            _validator = validator;
        }

        public async Task<OrderResult> SendOrderAsync(OrderDto orderDto)
        {
            var validationOderDto = await _validator.ValidateAsync(orderDto);
            if(!validationOderDto.IsValid)
            {
                return new OrderResult(false, string.Join("; ", validationOderDto.Errors.Select(e => e.ErrorMessage)));
            }

            // validações básicas (opcional)
            var response = await _fixOrderSender.FixOrderSenderAsync(orderDto);
            return response;
        }
    }
}
