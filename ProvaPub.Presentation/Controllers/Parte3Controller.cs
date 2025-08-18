using Microsoft.AspNetCore.Mvc;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;


namespace ProvaPub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Parte3Controller : ControllerBase
    {
        private readonly IOrderService _orderService;

        public Parte3Controller(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("orders")]
        public async Task<Order> PlaceOrder(string paymentMethod, decimal paymentValue, int customerId)
        {
            return await _orderService.PayOrder(paymentMethod, paymentValue, customerId);
        }
    }
}