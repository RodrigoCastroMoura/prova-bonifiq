namespace ProvaPub.App.PaymentStrategies
{
    public interface IPaymentStrategy
    {
        Task<bool> ProcessPaymentAsync(decimal amount);
        string PaymentMethod { get; }
    }

    public class PixPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethod => "pix";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            // Simula processamento PIX
            await Task.Delay(100);

            // Aqui viria a integração real com o PIX
            Console.WriteLine($"Processando pagamento PIX de R$ {amount:F2}");

            return true;
        }
    }

    public class CreditCardPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethod => "creditcard";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            // Simula processamento cartão de crédito
            await Task.Delay(200);

            // Aqui viria a integração real com operadora de cartão
            Console.WriteLine($"Processando pagamento cartão de crédito de R$ {amount:F2}");

            return true;
        }
    }

    public class PaypalPaymentStrategy : IPaymentStrategy
    {
        public string PaymentMethod => "paypal";

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            // Simula processamento PayPal
            await Task.Delay(150);

            // Aqui viria a integração real com PayPal
            Console.WriteLine($"Processando pagamento PayPal de R$ {amount:F2}");

            return true;
        }
    }

    public interface IPaymentStrategyFactory
    {
        IPaymentStrategy GetStrategy(string paymentMethod);
        IEnumerable<string> GetAvailablePaymentMethods();
    }

    public class PaymentStrategyFactory : IPaymentStrategyFactory
    {
        private readonly Dictionary<string, Func<IPaymentStrategy>> _strategies;

        public PaymentStrategyFactory()
        {
            _strategies = new Dictionary<string, Func<IPaymentStrategy>>(StringComparer.OrdinalIgnoreCase)
            {
                { "pix", () => new PixPaymentStrategy() },
                { "creditcard", () => new CreditCardPaymentStrategy() },
                { "paypal", () => new PaypalPaymentStrategy() }
            };
        }

        public IPaymentStrategy GetStrategy(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod) ||
                !_strategies.TryGetValue(paymentMethod, out var strategyFactory))
            {
                throw new NotSupportedException($"Payment method '{paymentMethod}' is not supported. " +
                    $"Available methods: {string.Join(", ", GetAvailablePaymentMethods())}");
            }

            return strategyFactory();
        }

        public IEnumerable<string> GetAvailablePaymentMethods()
        {
            return _strategies.Keys;
        }
    }
}