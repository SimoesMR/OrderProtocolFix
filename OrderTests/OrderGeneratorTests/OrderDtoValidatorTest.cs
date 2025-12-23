using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Validator;

namespace OrderTests.OrderGeneratorTests
{
    public class OrderDtoValidatorTest
    {
        private readonly OrderDtoValidator _validator;
        public OrderDtoValidatorTest()
        {
            _validator = new OrderDtoValidator();
        }
        [Fact]
        public void SuccessfulValidation()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, 150.00m);
           
            var result = _validator.Validate(orderDto);
            
            Assert.True(result.IsValid);
        }

        [Fact]
        public void FailedValidation_InvalidSymbol()
        {
            var orderDto = new OrderDto("TECN3", "BUY", 100, 150.00m);
            // Act
            var result = _validator.Validate(orderDto);
            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Symbol");
        }

        [Fact]
        public void FailedValidation_InvalidSide()
        {
            var orderDto = new OrderDto("PETR4", "HOLD", 100, 150.00m);
      
            var result = _validator.Validate(orderDto);
       
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Side");
        }

        [Fact]
        public void FailedValidation_InvalidNegativeQuantity()
        {
            var orderDto = new OrderDto("PETR4", "BUY", -100, 150.00m);
      
            var result = _validator.Validate(orderDto);
   
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
        }

        [Fact]
        public void FailedValidation_InvalidMinQuantity()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 0, 150.00m);

            var result = _validator.Validate(orderDto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
        }

        [Fact]
        public void FailedValidation_InvalidMaxQuantity()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100_000, 150.00m);
         
            var result = _validator.Validate(orderDto);
     
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
        }

        [Fact]
        public void FailedValidation_InvalidNegativePrice()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, -150.00m);

            var result = _validator.Validate(orderDto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        }

        [Fact]
        public void FailedValidation_InvalidHighPrice()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, 1_500.00m);

            var result = _validator.Validate(orderDto);
         
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        }

        [Fact]
        public void FailedValidation_InvalidPriceNotMultipleOfPointZeroOne()
        {
            var orderDto = new OrderDto("PETR4", "BUY", 100, 150.005m);

            var result = _validator.Validate(orderDto);
         
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        }

        [Fact]
        public void FailedValidation_MultipleErrors()
        {
            var orderDto = new OrderDto("", "HOLD", -10, 1_500.005m);

            var result = _validator.Validate(orderDto);
          
            Assert.False(result.IsValid);
            Assert.Equal(6, result.Errors.Count);
        }

        [Fact]
        public void SuccessfulValidation_BorderValues()
        {
            var orderDto = new OrderDto("VALE3", "SELL", 99_999, 999.99m);
           
            var result = _validator.Validate(orderDto);
          
            Assert.True(result.IsValid);
        }
    }
}
