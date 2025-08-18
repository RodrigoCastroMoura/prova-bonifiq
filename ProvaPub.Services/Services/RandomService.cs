using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;

namespace ProvaPub.App.Services
{
    public class RandomService : IRandomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RandomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetRandom()
        {
            int number;
            int attempts = 0;
            const int maxAttempts = 1000;

            do
            {
                // Gera seed único baseado no tempo atual
                var seed = DateTime.Now.Millisecond + Environment.TickCount + Thread.CurrentThread.ManagedThreadId;
                number = new Random(seed).Next(100);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException(
                        "Não foi possível gerar um número único após múltiplas tentativas");
                }
            }
            while (await _unitOfWork.RandomNumbers.AnyAsync(n => n.Number == number));

            // Adiciona o número único
            var randomNumber = new RandomNumber { Number = number };
            await _unitOfWork.RandomNumbers.AddAsync(randomNumber);
            await _unitOfWork.SaveChangesAsync();

            return number;
        }
    }
}