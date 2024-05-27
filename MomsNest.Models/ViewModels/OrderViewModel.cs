using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models.ViewModels
{
	public class OrderViewModel
	{
		public OrderHeader OrderHeader { get; set; }
		
		public IEnumerable<OrderDetails> OrderDetails { get; set; }
       
		[ValidateNever]
        public IEnumerable<Offer> Offers { get; set; }
    }
}
