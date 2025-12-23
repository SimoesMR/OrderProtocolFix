using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderGenerator.Application.Dto
{
    public class OrderDto
    {
        public string Symbol { get; private set; }
        public string Side { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }

        public OrderDto(string symbol, string side, int quantity, decimal price)
        {
            Symbol = symbol;
            Side = side;
            Quantity = quantity;
            Price = price;
        }
    }
}
