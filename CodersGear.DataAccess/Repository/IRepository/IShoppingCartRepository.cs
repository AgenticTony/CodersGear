using CodersGear.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart obj);
        IEnumerable<ShoppingCart> GetBySessionId(string sessionId);
        int MergeSessionCartToUserCart(string sessionId, string userId);
    }
}
