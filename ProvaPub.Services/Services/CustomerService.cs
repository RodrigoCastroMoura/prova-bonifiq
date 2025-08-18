using Microsoft.EntityFrameworkCore;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;
using ProvaPub.Infrastructure.Repository;

namespace ProvaPub.App.Services
{

 

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public CustomerService(TestDbContext ctx, IDateTimeProvider dateTimeProvider = null) : base(ctx)
        {
            _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        }

        public CustomerList ListCustomers(int page)
        {
            var pagedResult = ListPaged(page);

            return new CustomerList
            {
                Customers = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                HasNext = pagedResult.HasNext
            };
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));

            if (purchaseValue <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseValue));

            //Business Rule: Non registered Customers cannot purchase
            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null) throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            //Business Rule: A customer can purchase only a single time per month
            var baseDate = _dateTimeProvider.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _ctx.Orders.CountAsync(s => s.CustomerId == customerId && s.OrderDate >= baseDate);
            if (ordersInThisMonth > 0)
                return false;

            //Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
            var haveBoughtBefore = await _ctx.Customers.CountAsync(s => s.Id == customerId && s.Orders.Any());
            if (haveBoughtBefore == 0 && purchaseValue > 100)
                return false;

            //Business Rule: A customer can purchases only during business hours and working days
            var currentTime = _dateTimeProvider.UtcNow;
            if (currentTime.Hour < 8 || currentTime.Hour > 18 || currentTime.DayOfWeek == DayOfWeek.Saturday || currentTime.DayOfWeek == DayOfWeek.Sunday)
                return false;

            return true;
        }
    }
}