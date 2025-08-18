using Microsoft.EntityFrameworkCore;
using ProvaPub.Domain.Models;
using ProvaPub.Infrastructure.Repository;

namespace ProvaPub.App.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected readonly TestDbContext _ctx;
        protected readonly DbSet<T> _dbSet;

        public BaseService(TestDbContext ctx)
        {
            _ctx = ctx;
            _dbSet = _ctx.Set<T>();
        }

        public virtual PagedList<T> ListPaged(int page, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalCount = _dbSet.Count();
            var skip = (page - 1) * pageSize;

            var items = _dbSet
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new PagedList<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                HasNext = skip + pageSize < totalCount
            };
        }
    }
}