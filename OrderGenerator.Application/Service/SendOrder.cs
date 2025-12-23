using FluentValidation;
using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Interfaces;
using Serilog;

namespace OrderGenerator.Application.Service
{
    public class SendOrder : ISendOrder
    {
        private readonly IFixOrderSender _fixOrderSender;
        private readonly IValidator<OrderDto> _validator;
        private readonly ILogger _logger;

        public SendOrder(IFixOrderSender sender, IValidator<OrderDto> validator)
        {
            _fixOrderSender = sender;
            _validator = validator;
            _logger = Log.ForContext<SendOrder>();
        }

        public async Task<OrderResult> SendOrderAsync(OrderDto orderDto)
        {
            _logger.Information("Recebida ordem: {Symbol} {Side} {Quantity}x @ R$ {Price:N2}",
                orderDto.Symbol, orderDto.Side, orderDto.Quantity, orderDto.Price);

            var validationOderDto = await _validator.ValidateAsync(orderDto);
            if(!validationOderDto.IsValid)
            {
                _logger.Warning("Validação falhou: {Errors}",
                    string.Join("; ", validationOderDto.Errors.Select(e => e.ErrorMessage)));

                return new OrderResult(false, string.Join("; ", validationOderDto.Errors.Select(e => e.ErrorMessage)));
            }

            _logger.Debug("Ordem validada, enviando para FIX..."); 

            var response = await _fixOrderSender.FixOrderSenderAsync(orderDto);

            _logger.Information("Resposta FIX: Accepted={Accepted} Message={Message}", response.Accepted, response.Message);
            return response;
        }
    }
}
