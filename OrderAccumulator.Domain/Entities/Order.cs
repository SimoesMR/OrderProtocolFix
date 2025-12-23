using OrderAccumulator.Domain.ValueObject;

namespace OrderAccumulator.Domain.Entities
{
    public class Order
    {
        public Symbol Symbol { get; private  set; }
        public OrderSideType Side { get; private  set; }
        public Money Price { get; private set; }
        public int Quantity { get; private set; }
        public string ClOrdID { get; private set; }
        public decimal FinancialValue => Price.Value * Quantity;

        public Order(Symbol symbol, OrderSideType side, Money price, int quantity, string cl0rdID)
        {
            if(price.Value > 1_000)
                throw new ArgumentException("Price cannot be higher than 1000");
            if(quantity <= 0 || quantity > 100_000)
                throw new ArgumentException("Quantity must be between 1 and 1,000,000");

            Symbol = symbol;
            Side = side;
            Price = price;
            Quantity = quantity;
            ClOrdID = cl0rdID;
        }
    }
}
