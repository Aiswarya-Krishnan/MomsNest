using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models
{
    public class WishList
    {


        public int WishListId { get; set; }
        public string ApplicationUserID { get; set; }
        [ForeignKey("ApplicationUserID")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public int Product_ID { get; set; }
        [ForeignKey("Product_ID")]
        [ValidateNever]
        public Product Product { get; set; }

        public string ProdctName { get; set; }
        public string Description { get; set; }

        [NotMapped]
        public decimal Price { get; set; }



    }
}
