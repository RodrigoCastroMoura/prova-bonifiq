using Microsoft.EntityFrameworkCore.Storage;
using ProvaPub.Domain.Interfaces.Repository;
using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Infrastructure.Context;
using ProvaPub.Infrastructure.Repository;

namespace ProvaPub.Infrastructure.Unit
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TestDbContext _context;
        private IDbContextTransaction? _transaction;

        private IRepository<Customer>? _customers;
        private IRepository<Product>? _products;
        private IRepository<Order>? _orders;
        private IRepository<RandomNumber>? _randomNumbers;

        public UnitOfWork(TestDbContext context)
        {
            _context = context;
        }

        public IRepository<Customer> Customers =>
            _customers ??= new Repository<Customer>(_context);

        public IRepository<Product> Products =>
            _products ??= new Repository<Product>(_context);

        public IRepository<Order> Orders =>
            _orders ??= new Repository<Order>(_context);

        public IRepository<RandomNumber> RandomNumbers =>
            _randomNumbers ??= new Repository<RandomNumber>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}