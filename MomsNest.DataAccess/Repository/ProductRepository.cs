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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public void Update(Product obj)
        {
            var objFromDb=_context.Products.FirstOrDefault(u=>u.ProductId == obj.ProductId);
            if(objFromDb != null)
            {
                objFromDb.ProductName=obj.ProductName;
                objFromDb.Description=obj.Description;
                objFromDb.Price=obj.Price;
                objFromDb.StockQuantity=obj.StockQuantity;
                objFromDb.Brand=obj.Brand;
                objFromDb.Color=obj.Color;
                objFromDb.Material=obj.Material;
                objFromDb.Weight=obj.Weight;
                objFromDb.CategoryID=obj.CategoryID;
                objFromDb.ProductImages = obj.ProductImages;
                objFromDb.Discount = obj.Discount;
                //if(obj.ImageUrl !=null)
                //{
                //    objFromDb.ImageUrl=obj.ImageUrl;
                //}
            }
        }
    }
}
