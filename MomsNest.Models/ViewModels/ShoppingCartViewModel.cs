using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models.ViewModels
{
    public class ShoppingCartViewModel  
    {
        public IEnumerable<ShoppingCart> ShoppingCartList {  get; set; }
        public OrderHeader OrderHeader { get; set; }

        public IEnumerable<Coupons> Coupons { get; set; }

        [ValidateNever]
        public IEnumerable<Offer> Offers { get; set; }

        //public decimal OrderTotal {  get; set; }
    }
}
