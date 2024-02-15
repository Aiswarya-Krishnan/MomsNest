using Microsoft.EntityFrameworkCore;
using MomsNest.DataAccess.Data;
using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository
{
    public class OrderHeaderRepository:Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly AppDbContext _context;

    public OrderHeaderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }



    public void Update(OrderHeader obj)
    {
        _context.OrderHeader.Update(obj);
    }
}
}
