namespace ProvaPub.Domain.Services
{
    public interface IRandomService
    {
        Task<int> GetRandom();
    }
}
