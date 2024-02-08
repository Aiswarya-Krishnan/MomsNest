using MomsNest.DataAccess.Data;
using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext context;


        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public UnitOfWork(AppDbContext context)
        {
            this.context = context;
            Category = new CategoryRepository(context);
            Product = new ProductRepository(context);
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
