using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;
using ProvaPub.Infrastructure.Repository;


namespace ProvaPub.App.Services
{
    public class ProductService : BaseService<Product>, IProductService
    {
        public ProductService(TestDbContext ctx) : base(ctx)
        {
        }

        public ProductList ListProducts(int page)
        {
            var pagedResult = ListPaged(page);

            return new ProductList
            {
                Products = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                HasNext = pagedResult.HasNext
            };
        }
    }
}