using MomsNest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.DataAccess.Repository.Interfaces
{
    public interface IWishListRepository: IRepository<WishList>
    {
        void Update(WishList obj);

    }
}
