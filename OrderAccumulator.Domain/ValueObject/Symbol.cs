namespace OrderAccumulator.Domain.ValueObject
{
    public class Symbol
    {
        private static readonly HashSet<string> ValidSymbols = new() { "PETR4", "VALE3", "VIIA4" };

        public string Value { get; private set; }

        public Symbol(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Symbol cannot be null or empty.");

            if (!ValidSymbols.Contains(value))
                throw new ArgumentException($"Symbol must be one of the following: {string.Join(", ", ValidSymbols)}");

            Value = value;
        }
    }
}
