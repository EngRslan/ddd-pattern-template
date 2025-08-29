using Engrslan.DependencyInjection;
using Engrslan.Repositories;
using Engrslan.Sample.Entities;
using Engrslan.Sample.Interfaces;

namespace Engrslan.Sample.Repositories;

public class ProductRepository : Repository<Product,Guid>, IProductRepository, IScopedService
{
    public ProductRepository(ApplicationDataContext context) : base(context)
    {
    }
}