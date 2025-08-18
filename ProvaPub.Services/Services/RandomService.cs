using Microsoft.EntityFrameworkCore;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;
using ProvaPub.Infrastructure.Repository;

namespace ProvaPub.App.Services
{
    public class RandomService : IRandomService
    {
        private readonly TestDbContext _ctx;

        public RandomService(TestDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int> GetRandom()
        {
            int number;
            int attempts = 0;
            const int maxAttempts = 1000; 

            do
            {
                var seed = DateTime.Now.Millisecond + Environment.TickCount;
                number = new Random(seed).Next(100);
                attempts++;

                if (attempts >= maxAttempts)
                {
                    throw new InvalidOperationException("Não foi possível gerar um número único após múltiplas tentativas");
                }
            }
            while (await _ctx.Numbers.AnyAsync(n => n.Number == number));

            _ctx.Numbers.Add(new RandomNumber() { Number = number });
            await _ctx.SaveChangesAsync();
            return number;
        }
    }
}