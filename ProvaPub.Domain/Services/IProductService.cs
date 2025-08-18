using ProvaPub.Domain.Models;

namespace ProvaPub.Domain.Services
{
    public interface IProductService
    {
        ProductList ListProducts(int page);
    }
}
