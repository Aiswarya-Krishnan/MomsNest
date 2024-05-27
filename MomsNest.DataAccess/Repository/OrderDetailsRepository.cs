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
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly AppDbContext _context;

        public OrderDetailsRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public void Update(OrderDetails obj)
        {
            _context.OrderDetails.Update(obj);
        }
    }
}
