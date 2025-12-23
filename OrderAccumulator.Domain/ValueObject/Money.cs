namespace OrderAccumulator.Domain.ValueObject
{
    public class Money
    {
        public decimal Value { get; private set; }
        //possible to add currency later

        public Money(decimal value)
        {
            if (value < 0)
                throw new ArgumentException("Monetary value cannot be negative.");
            Value = value;
        }
    }
}
