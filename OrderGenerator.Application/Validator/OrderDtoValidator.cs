using OrderGenerator.Application.Dto;
using FluentValidation;

namespace OrderGenerator.Application.Validator
{
    public class OrderDtoValidator : AbstractValidator<OrderDto>
    {
        private static readonly string[] ValidSymbols = { "PETR4", "VALE3", "VIIA4" };
        private static readonly string[] ValidSides = { "BUY", "SELL" };

        //validação do dto de ordem
        public OrderDtoValidator() {
            // Symbol
            RuleFor(x => x.Symbol)
                .NotEmpty()
                .WithMessage("Símbolo é obrigatório")
                .Must(s => ValidSymbols.Contains(s))
                .WithMessage($"Símbolo inválido. Use: {string.Join(", ", ValidSymbols)}");

            // Side
            RuleFor(x => x.Side)
                .NotEmpty()
                .WithMessage("Lado é obrigatório")
                .Must(s => ValidSides.Contains(s))
                .WithMessage("Lado inválido. Use Compra ou Venda");

            // Quantity
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantidade deve ser maior que zero")
                .LessThan(100_000)
                .WithMessage("Quantidade deve ser menor que 100.000");

            // Price
            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Preço deve ser maior que zero")
                .LessThan(1_000)
                .WithMessage("Preço deve ser menor que 1.000")
                .Must(BeMultipleOfPointZeroOne)
                .WithMessage("Preço deve ser múltiplo de 0.01");
        }

        private bool BeMultipleOfPointZeroOne(decimal price)
        {
            var rounded = Math.Round(price, 2);
            return price == rounded;
        }
    }
}
