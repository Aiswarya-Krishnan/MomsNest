using MomsNest.DataAccess.Data;
using MomsNest.DataAccess.Repository.Interfaces;
using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly AppDbContext _context;

        public ProductImageRepository   (AppDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(ProductImage obj)
        {
            _context.ProductImages.Update(obj);
        }
    }
}
