using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MomsNest.Models.Offer;

namespace MomsNest.Models.ViewModels
{
    public class OfferViewModel
    {
       
        public Offer Offer { get; set; }

        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }

        [ValidateNever]
        public IEnumerable<Product> Products { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedProductId { get; set; }
    }
}
