using Microsoft.EntityFrameworkCore;
using MomsNest.Models;

namespace MomsNest.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
                
        }
        public DbSet<Category> Categories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId=1,Name="Footwear",DisplayOrder=1},
                new Category { CategoryId = 2, Name = "Toys", DisplayOrder = 2 },
                new Category { CategoryId =3, Name = "Hygiene", DisplayOrder = 3 }

                );
            
        }

    }
}
