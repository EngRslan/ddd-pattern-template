using Engrslan.Interfaces;
using Engrslan.Sample.Entities;

namespace Engrslan.Sample.Interfaces;

public interface IProductRepository : IRepository<Product, Guid>
{
    
}