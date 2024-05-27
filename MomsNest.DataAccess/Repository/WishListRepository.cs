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
    public class WishListRepository: Repository<WishList>, IWishListRepository
    {
        private readonly AppDbContext _context;

        public WishListRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public void Update(WishList obj)
        {
            _context.WishList.Update(obj);
        }
    }
}
