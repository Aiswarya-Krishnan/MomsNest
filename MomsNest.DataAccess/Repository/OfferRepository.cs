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
    public class OfferRepository : Repository<Offer>, IOfferRepository
    {
        private readonly AppDbContext _context;

        public OfferRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public void Update(Offer obj)
        {
            _context.Offers.Update(obj);
        }
    }
}