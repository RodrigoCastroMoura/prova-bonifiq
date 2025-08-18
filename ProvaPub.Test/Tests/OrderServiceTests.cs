using Microsoft.EntityFrameworkCore;
using ProvaPub.App.PaymentStrategies;
using ProvaPub.App.Services;
using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Infrastructure.Context;
using ProvaPub.Infrastructure.Unit;

using Xunit;

namespace ProvaPub.Tests
{
    public class OrderServiceTests : IDisposable
    {
        private readonly TestDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentStrategyFactory _paymentStrategyFactory;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _paymentStrategyFactory = new PaymentStrategyFactory();
            _orderService = new OrderService(_unitOfWork, _paymentStrategyFactory);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Customers.Add(new Customer { Id = 1, Name = "Test Customer" });
            _context.SaveChanges();
        }


        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task PayOrder_InvalidPaymentMethod_ThrowsArgumentException(string paymentMethod)
        {
            // Arrange
            const decimal paymentValue = 100.50m;
            const int customerId = 1;

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() =>
                _orderService.PayOrder(paymentMethod, paymentValue, customerId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(-100.50)]
        public async Task PayOrder_InvalidPaymentValue_ThrowsArgumentException(decimal paymentValue)
        {
            // Arrange
            const string paymentMethod = "pix";
            const int customerId = 1;

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() =>
                _orderService.PayOrder(paymentMethod, paymentValue, customerId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async Task PayOrder_InvalidCustomerId_ThrowsArgumentException(int customerId)
        {
            // Arrange
            const string paymentMethod = "pix";
            const decimal paymentValue = 100.50m;

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentException>(() =>
                _orderService.PayOrder(paymentMethod, paymentValue, customerId));
        }

        [Fact]
        public async Task PayOrder_NonExistentCustomer_ThrowsInvalidOperationException()
        {
            // Arrange
            const string paymentMethod = "pix";
            const decimal paymentValue = 100.50m;
            const int nonExistentCustomerId = 999;

            // Act & Assert
            var exception = await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.PayOrder(paymentMethod, paymentValue, nonExistentCustomerId));

            Xunit.Assert.Contains("Customer with ID 999 not found", exception.Message);
        }

      
        [Fact]
        public async Task InsertOrder_ValidOrder_SavesAndConvertsTimezone()
        {
            // Arrange
            var utcTime = new DateTime(2023, 6, 15, 15, 30, 0, DateTimeKind.Utc); // 15:30 UTC
            var order = new Order
            {
                Value = 150.75m,
                CustomerId = 1,
                OrderDate = utcTime
            };

            // Act
            var result = await _orderService.InsertOrder(order);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.True(result.Id > 0);

            // Verifica se o horário foi convertido para BRT (UTC-3)
            var expectedBrazilTime = utcTime.AddHours(-3); // 12:30 BRT
            Xunit.Assert.Equal(expectedBrazilTime, result.OrderDate);
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
            _context.Dispose();
        }
    }
}