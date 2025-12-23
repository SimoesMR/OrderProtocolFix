using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderGenerator.Application.Dto
{
    public class OrderResult
    {
        public bool Accepted { get; private set; }
        public string Message { get; private set; }

        public OrderResult(bool accepted, string message)
        {
            Accepted = accepted;
            Message = message;
        }

    }
}
