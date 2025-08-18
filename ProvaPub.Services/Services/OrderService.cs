using ProvaPub.App.PaymentStrategies;
using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;

namespace ProvaPub.App.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentStrategyFactory _paymentStrategyFactory;

        public OrderService(IUnitOfWork unitOfWork, IPaymentStrategyFactory paymentStrategyFactory)
        {
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
        }

        public async Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("Payment method cannot be null or empty", nameof(paymentMethod));

            if (paymentValue <= 0)
                throw new ArgumentException("Payment value must be greater than zero", nameof(paymentValue));

            if (customerId <= 0)
                throw new ArgumentException("Customer ID must be greater than zero", nameof(customerId));

            // Verifica se o cliente existe
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {customerId} not found");

            try
            {
                // Inicia transação
                await _unitOfWork.BeginTransactionAsync();

                // Processa o pagamento usando Strategy Pattern
                var paymentStrategy = _paymentStrategyFactory.GetStrategy(paymentMethod);
                var paymentSuccessful = await paymentStrategy.ProcessPaymentAsync(paymentValue);

                if (!paymentSuccessful)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Payment processing failed");
                }

                // Cria o pedido
                var order = new Order
                {
                    Value = paymentValue,
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow // Salva sempre como UTC
                };

                var insertedOrder = await InsertOrder(order);

                // Confirma a transação
                await _unitOfWork.CommitTransactionAsync();

                return insertedOrder;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Order> InsertOrder(Order order)
        {
            var insertedOrder = await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Converte para fuso horário brasileiro (UTC-3) antes de retornar
            insertedOrder.OrderDate = ConvertToBrazilianTime(insertedOrder.OrderDate);

            return insertedOrder;
        }

        private static DateTime ConvertToBrazilianTime(DateTime utcDateTime)
        {
            var brazilTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                "BRT",
                new TimeSpan(-3, 0, 0),
                "Brazil Time",
                "Brazil Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, brazilTimeZone);
        }
    }
}