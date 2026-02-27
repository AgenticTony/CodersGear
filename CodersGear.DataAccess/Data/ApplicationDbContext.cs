using CodersGear.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodersGear.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
               new Category { CategoryId = 1, Name = "T-shirts", DisplayOrder = 1 },
               new Category { CategoryId = 2, Name = "Hoodies", DisplayOrder = 2 },
               new Category { CategoryId = 3, Name = "Mugs", DisplayOrder = 3 },
               new Category { CategoryId = 4, Name = "Accessoriess", DisplayOrder = 4 }
               );
            modelBuilder.Entity<Product>().HasData(
                    new Product
                    {

                        ProductId = 1,
                        ProductName = "Coder's Gear T-shirt",
                        Description = "A comfortable and stylish t-shirt for coders.",
                        ListPrice = 28.99m,
                        Price = 23.99m,
                        Price50 = 21.99m,
                        Price100 = 18.99m,
                        CategoryId = 1,
                        UPC = "123456789012",
                        ImageUrl = "/images/product/8ab587da-7087-4790-848b-44882097a3e0.jpg"
                    },

                    new Product
                    {
                        ProductId = 2,
                        ProductName = "Coder's Gear Hoodie",
                        Description = "A cozy hoodie for coders to stay warm while coding.",
                        ListPrice = 44.99m,
                        Price = 39.99m,
                        Price50 = 36.99m,
                        Price100 = 32.99m,
                        CategoryId = 2,
                        UPC = "123456789013",
                        ImageUrl = "/images/product/a49a695e-cb70-442a-970b-0a4aed5e064d.jpg"
                    },
                    new Product
                    {
                        ProductId = 3,
                        ProductName = "Coder's Gear Mug",
                        Description = "A mug for coders to enjoy their favorite beverages while coding.",
                        ListPrice = 19.99m,
                        Price = 14.99m,
                        Price50 = 12.99m,
                        Price100 = 9.99m,
                        CategoryId = 3,
                        UPC = "123456789014",
                        ImageUrl = "/images/product/da05ef9c-05f7-4554-a5cf-02088b53c821.jpg"
                     },
                    new Product
                    {
                        ProductId = 4,
                        ProductName = "Coder's Gear Laptop Sleeve",
                        Description = "A protective laptop sleeve for coders to carry their laptops in style.",
                        ListPrice = 29.99m,
                        Price = 24.99m,
                        Price50 = 22.99m,
                        Price100 = 19.99m,
                        CategoryId = 4,
                        UPC = "123456789015",
                        ImageUrl = "/images/product/e603a324-fbf8-4c9a-9e5c-0f7ff3141cf4.jpg"
                    }
                );

        }

    }
}
