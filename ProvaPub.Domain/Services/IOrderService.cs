using ProvaPub.Domain.Models;

namespace ProvaPub.Domain.Services
{
    public interface IOrderService
    {
        Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId);
        Task<Order> InsertOrder(Order order);
    }
}
