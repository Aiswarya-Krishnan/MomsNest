using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using MomsNest.Models;

namespace MomsNest.DataAccess.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
                
        }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers {  get; set; }  
        public DbSet<ShoppingCart> shoppingCarts { get; set; }

        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }

        public DbSet<Coupons> Coupons { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<WishList> WishList { get; set; }

        public DbSet<Offer> Offers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId=1,Name="Footwear",DisplayOrder=1},
                new Category { CategoryId = 2, Name = "Toys", DisplayOrder = 2 },
                new Category { CategoryId =3, Name = "Hygiene", DisplayOrder = 3 }

                );

            modelBuilder.Entity<Product>().HasData(

                new Product
                {
                    ProductId = 1,
                    ProductName = "Cuddly Teddy Bear",
                    Description = "Soft and cuddly teddy bear for newborns",
                    Price = 19.99m,
                    StockQuantity = 50,
                    Brand = "SnuggleCo",
                    Color = "Brown",
                    Material = "Plush",
                    Weight = 0.5m,
                    CategoryID=2,


                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Baby Onesie Set",
                    Description = "Adorable onesie set for infants",
                    Price = 29.99m,
                    StockQuantity = 30,
                    Brand = "TinyTots",
                    Color = "Pastel",
                    Material = "Cotton",
                    Weight = 0.3m,
                    CategoryID = 2,
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Interactive Baby Mobile",
                    Description = "Colorful mobile with music for crib entertainment",
                    Price = 39.99m,
                    StockQuantity = 20,
                    Brand = "PlayfulHands",
                    Color = "Multicolor",
                    Material = "Plastic",
                    Weight = 0.8m,
                    CategoryID = 2,
                },
                new Product
                {
                    ProductId = 4,
                    ProductName = "Baby Feeding Set",
                    Description = "Complete feeding set for infants",
                    Price = 49.99m,
                    StockQuantity = 25,
                    Brand = "MunchieMunch",
                    Color = "Blue",
                    Material = "BPA-Free Plastic",
                    Weight = 0.6m,
                    CategoryID = 2,
                },
                new Product
                {
                    ProductId = 5,
                    ProductName = "Educational Baby Book",
                    Description = "Interactive and colorful book for early learning",
                    Price = 14.99m,
                    StockQuantity = 40,
                    Brand = "LearnSmart",
                    Color = "Yellow",
                    Material = "Cardboard",
                    Weight = 0.2m,
                    CategoryID = 2,

                }


                );
            modelBuilder.Entity<Category>()
              .HasIndex(c => c.Name)
                .IsUnique();

          
        }

    }
}
