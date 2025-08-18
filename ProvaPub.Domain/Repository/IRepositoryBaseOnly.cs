

using ProvaPub.Domain.Models;

namespace ProvaPub.Domain.Repository
{
    public interface IRepositoryBaseOnly<TEntity> where TEntity : class
    {
        PagedList<TEntity> ListPaged(int page, int pageSize = 10);
    }
}
