using MomsNest.DataAccess.Data;
using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly AppDbContext _context;

        public ApplicationUserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }


    }
}
