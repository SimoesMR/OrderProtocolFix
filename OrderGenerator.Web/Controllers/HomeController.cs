using Microsoft.AspNetCore.Mvc;
using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Interfaces;
using OrderGenerator.Fix;
using OrderGenerator.Web.Models;
using QuickFix.Fields;
using QuickFix.FIX44;
using System.Diagnostics;

namespace OrderGenerator.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISendOrder _sendOrder;


        public HomeController(ILogger<HomeController> logger, ISendOrder sendOrder)
        {
            _logger = logger;
            _sendOrder = sendOrder;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOrder([FromBody] OrderDto order)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Ordem inválida");
                return BadRequest(ModelState);
            }
             
            OrderResult result = await _sendOrder.SendOrderAsync(order);

            if (!result.Accepted)
                return UnprocessableEntity(new { message = result.Message });

            return Ok(result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
