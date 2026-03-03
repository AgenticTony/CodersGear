using CodersGear.DataAccess.Data;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models ;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodersGear.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {


            var objFromDb = _db.Products.FirstOrDefault(s => s.ProductId == obj.ProductId);
            if (objFromDb != null)
            {
                objFromDb.ProductName = obj.ProductName;
                objFromDb.Description = obj.Description;
                objFromDb.Price = obj.Price;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price50 = obj.Price50;
                objFromDb.Price100 = obj.Price100;
                objFromDb.UPC = obj.UPC;
                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }

                // Update Printify-specific fields
                objFromDb.AdditionalImages = obj.AdditionalImages;
                objFromDb.PrintifyOptionsData = obj.PrintifyOptionsData;
                objFromDb.PrintifyVariantData = obj.PrintifyVariantData;
                objFromDb.IsPrintifyProduct = obj.IsPrintifyProduct;
                objFromDb.PrintifyProductId = obj.PrintifyProductId;
                objFromDb.PrintifyShopId = obj.PrintifyShopId;
                objFromDb.LastSyncedAt = obj.LastSyncedAt;
                objFromDb.Visible = obj.Visible;
            }
        }
    }
}
