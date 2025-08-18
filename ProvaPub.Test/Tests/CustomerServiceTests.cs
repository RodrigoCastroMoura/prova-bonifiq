using Microsoft.EntityFrameworkCore;
using ProvaPub.App.Services;
using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;
using ProvaPub.Infrastructure.Context;
using ProvaPub.Infrastructure.Unit;

using Xunit;

namespace ProvaPub.Tests
{
    public class CustomerServiceTests : IDisposable
    {
        private readonly TestDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MockDateTimeProvider _dateTimeProvider;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _dateTimeProvider = new MockDateTimeProvider();
            _customerService = new CustomerService(_unitOfWork, _dateTimeProvider);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Customers.AddRange(
                new Customer { Id = 1, Name = "Customer with orders" },
                new Customer { Id = 2, Name = "Customer without orders" }
            );

            _context.Orders.Add(new Order
            {
                Id = 1,
                CustomerId = 1,
                Value = 50,
                OrderDate = DateTime.UtcNow.AddDays(-20) // Pedido dentro do último mês
            });

            _context.SaveChanges();
        }

        [Fact]
        public async Task CanPurchase_InvalidCustomerId_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _customerService.CanPurchase(0, 100));

            await Xunit.Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _customerService.CanPurchase(-1, 100));
        }

        [Fact]
        public async Task CanPurchase_InvalidPurchaseValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _customerService.CanPurchase(1, 0));

            await Xunit.Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _customerService.CanPurchase(1, -10));
        }

        [Fact]
        public async Task CanPurchase_NonExistentCustomer_ThrowsInvalidOperationException()
        {
            // Arrange & Act & Assert
            var exception = await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
                _customerService.CanPurchase(999, 100));

            Xunit.Assert.Contains("Customer Id 999 does not exists", exception.Message);
        }

        [Fact]
        public async Task CanPurchase_CustomerWithOrderInLastMonth_ReturnsFalse()
        {
            // Arrange
            _dateTimeProvider.SetDateTime(DateTime.UtcNow);

            // Act
            var result = await _customerService.CanPurchase(1, 50);

            // Assert
            Xunit.Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_NewCustomerWithHighValue_ReturnsFalse()
        {
            // Arrange
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 15, 10, 0, 0, DateTimeKind.Utc)); // Quinta-feira, 10h

            // Act
            var result = await _customerService.CanPurchase(2, 150); // Valor maior que 100

            // Assert
            Xunit.Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_NewCustomerValidValueValidTime_ReturnsTrue()
        {
            // Arrange
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 15, 10, 0, 0, DateTimeKind.Utc)); // Quinta-feira, 10h

            // Act
            var result = await _customerService.CanPurchase(2, 80); // Valor menor que 100

            // Assert
            Xunit.Assert.True(result);
        }

        [Fact]
        public async Task CanPurchase_OutsideBusinessHours_ReturnsFalse()
        {
            // Arrange - Teste antes das 8h
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 15, 7, 0, 0, DateTimeKind.Utc));

            // Act
            var result = await _customerService.CanPurchase(2, 50);

            // Assert
            Xunit.Assert.False(result);

            // Arrange - Teste após as 18h
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 15, 19, 0, 0, DateTimeKind.Utc));

            // Act
            result = await _customerService.CanPurchase(2, 50);

            // Assert
            Xunit.Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_Weekend_ReturnsFalse()
        {
            // Arrange - Sábado
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 17, 10, 0, 0, DateTimeKind.Utc));

            // Act
            var result = await _customerService.CanPurchase(2, 50);

            // Assert
            Xunit.Assert.False(result);

            // Arrange - Domingo
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 18, 10, 0, 0, DateTimeKind.Utc));

            // Act
            result = await _customerService.CanPurchase(2, 50);

            // Assert
            Xunit.Assert.False(result);
        }

        [Theory]
        [InlineData(8)]  // 8h
        [InlineData(12)] // 12h
        [InlineData(18)] // 18h
        public async Task CanPurchase_DuringBusinessHours_ReturnsTrue(int hour)
        {
            // Arrange
            _dateTimeProvider.SetDateTime(new DateTime(2023, 6, 15, hour, 0, 0, DateTimeKind.Utc)); // Quinta-feira

            // Act
            var result = await _customerService.CanPurchase(2, 50);

            // Assert
            Xunit.Assert.True(result);
        }

    
        [Fact]
        public void ListCustomers_ReturnsPagedResult()
        {
            // Act
            var result = _customerService.ListCustomers(1);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.NotNull(result.Customers);
            Xunit.Assert.True(result.TotalCount > 0);
        }

        [Fact]
        public void ListCustomers_Page2_ReturnsCorrectResults()
        {
            // Arrange - Adiciona mais clientes para testar paginação
            for (int i = 3; i <= 25; i++)
            {
                _context.Customers.Add(new Customer { Id = i, Name = $"Customer {i}" });
            }
            _context.SaveChanges();

            // Act
            var page1 = _customerService.ListCustomers(1);
            var page2 = _customerService.ListCustomers(2);

            // Assert
            Xunit.Assert.NotEqual(page1.Customers.First().Id, page2.Customers.First().Id);
            Xunit.Assert.True(page1.HasNext);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            _context.Dispose();
        }
    }

    public class MockDateTimeProvider : IDateTimeProvider
    {
        private DateTime _dateTime = DateTime.UtcNow;

        public DateTime UtcNow => _dateTime;

        public void SetDateTime(DateTime dateTime)
        {
            _dateTime = dateTime;
        }
    }
}