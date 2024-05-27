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

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderFromDDb=_context.OrderHeader.FirstOrDefault(u=>u.OrderHeaderId== id);
            if(orderFromDDb!=null) 
            {
                orderFromDDb.OrderStatus= orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDDb.PaymentStatus= paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			var orderFromDDb = _context.OrderHeader.FirstOrDefault(u => u.OrderHeaderId == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDDb.SessionId = sessionId;
            }
			if (!string.IsNullOrEmpty(paymentIntentId))
			{
				orderFromDDb.PaymentIntentId = paymentIntentId;
                orderFromDDb.OrderDate = DateTime.Now;
			}
		}
	}
}
