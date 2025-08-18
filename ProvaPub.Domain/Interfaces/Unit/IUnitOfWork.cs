using ProvaPub.Domain.Interfaces.Repository;
using ProvaPub.Domain.Models;

namespace ProvaPub.Domain.Interfaces.Unit
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<Product> Products { get; }
        IRepository<Order> Orders { get; }
        IRepository<RandomNumber> RandomNumbers { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}