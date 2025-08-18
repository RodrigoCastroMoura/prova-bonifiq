using Microsoft.AspNetCore.Mvc;
using ProvaPub.Domain.Services;


namespace ProvaPub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Parte4Controller : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public Parte4Controller(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("CanPurchase")]
        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            return await _customerService.CanPurchase(customerId, purchaseValue);
        }
    }
}