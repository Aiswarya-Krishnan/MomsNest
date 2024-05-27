using Microsoft.EntityFrameworkCore;
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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext context;


        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailsRepository OrderDetails { get; private set; }

        public ICouponRespository Coupons { get; private set; }
        public IProductImageRepository ProductImages { get; private set; }
        public IWishListRepository WishList { get; private set; }

        public IOfferRepository Offer { get; private set; } 

        public UnitOfWork(AppDbContext context)
        {
            this.context = context;
            Category = new CategoryRepository(context);
            Product = new ProductRepository(context);
            ShoppingCart=new ShoppingCartRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            OrderDetails = new OrderDetailsRepository(context);
            Coupons=new CouponRepository(context);
            ProductImages=new ProductImageRepository(context);
            WishList=new WishListRepository(context);
            Offer=new OfferRepository(context);
            
        }
        public DbContext Context => context;

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
