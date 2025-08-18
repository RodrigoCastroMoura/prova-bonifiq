using ProvaPub.Domain.Models;

namespace ProvaPub.Domain.Services;

public interface ICustomerService
{
    CustomerList ListCustomers(int page);
    Task<bool> CanPurchase(int customerId, decimal purchaseValue);
}
