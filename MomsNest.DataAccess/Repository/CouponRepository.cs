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
    public class CouponRepository: Repository<Coupons>, ICouponRespository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public void Update(Coupons obj)
        {
            _context.Coupons.Update(obj);
        }
    }
}
