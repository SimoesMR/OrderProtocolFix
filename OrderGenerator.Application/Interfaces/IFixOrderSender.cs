using OrderGenerator.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderGenerator.Application.Interfaces
{
    public interface IFixOrderSender
    {
        Task<OrderResult> FixOrderSenderAsync(OrderDto order);
    }
}
