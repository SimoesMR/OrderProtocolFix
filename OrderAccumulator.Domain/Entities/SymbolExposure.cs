using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderAccumulator.Domain.Entities
{
    public class SymbolExposure
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentExposure { get; set; }
        public List<Order> Orders { get; set; } = new ();
    }
}
