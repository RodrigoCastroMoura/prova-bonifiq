using ProvaPub.Domain.Interfaces.Unit;
using ProvaPub.Domain.Models;
using ProvaPub.Domain.Services;

namespace ProvaPub.App.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductList ListProducts(int page)
        {
            var pagedResult = _unitOfWork.Products.GetPaged(page);

            return new ProductList
            {
                Products = pagedResult.Items,
                TotalCount = pagedResult.TotalCount,
                HasNext = pagedResult.HasNext
            };
        }
    }
}