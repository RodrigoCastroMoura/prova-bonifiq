using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;

namespace ProvaPub.App.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CustomerService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public CustomerList ListCustomers(int page)
        {
            var pagedResult = _unitOfWork.Customers.GetPaged(page);

            return new CustomerList
            {
                Customers = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                HasNext = pagedResult.HasNext
            };
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            if (purchaseValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(purchaseValue));

            // Business Rule: Non registered Customers cannot purchase
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            // Business Rule: A customer can purchase only a single time per month
            var baseDate = _dateTimeProvider.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _unitOfWork.Orders.CountAsync(
                o => o.CustomerId == customerId && o.OrderDate >= baseDate);

            if (ordersInThisMonth > 0)
                return false;

            // Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
            var totalOrders = await _unitOfWork.Orders.CountAsync(o => o.CustomerId == customerId);
            if (totalOrders == 0 && purchaseValue > 100)
                return false;

            // Business Rule: A customer can purchases only during business hours and working days
            var currentTime = _dateTimeProvider.UtcNow;
            if (currentTime.Hour < 8 || currentTime.Hour > 18 ||
                currentTime.DayOfWeek == DayOfWeek.Saturday ||
                currentTime.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return true;
        }
    }
}