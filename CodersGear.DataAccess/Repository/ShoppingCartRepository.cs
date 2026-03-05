using CodersGear.DataAccess.Data;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models ;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.DataAccess.Repository
{
    public class    ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }

        public IEnumerable<ShoppingCart> GetBySessionId(string sessionId)
        {
            return _db.ShoppingCarts
                .Include(sc => sc.Product)
                .ThenInclude(p => p.Category)
                .Where(sc => sc.SessionId == sessionId)
                .AsNoTracking()
                .ToList();
        }

        public int MergeSessionCartToUserCart(string sessionId, string userId)
        {
            var sessionCarts = _db.ShoppingCarts
                .Where(sc => sc.SessionId == sessionId)
                .ToList();

            int itemsMerged = 0;

            foreach (var sessionCart in sessionCarts)
            {
                // Check if user already has this product AND variant in their cart
                var userCart = _db.ShoppingCarts
                    .FirstOrDefault(sc => sc.ApplicationUserId == userId &&
                                         sc.ProductId == sessionCart.ProductId &&
                                         sc.Size == sessionCart.Size &&
                                         sc.Color == sessionCart.Color);

                if (userCart != null)
                {
                    // Merge quantities (same product AND variant)
                    userCart.Count += sessionCart.Count;
                    userCart.OrderTotal += sessionCart.OrderTotal;
                    _db.ShoppingCarts.Update(userCart);
                }
                else
                {
                    // Transfer session cart to user cart (different variant or new product)
                    sessionCart.ApplicationUserId = userId;
                    sessionCart.SessionId = null; // Clear session ID
                    _db.ShoppingCarts.Update(sessionCart);
                }
                itemsMerged++;
            }

            _db.SaveChanges();
            return itemsMerged;
        }
    }
}
