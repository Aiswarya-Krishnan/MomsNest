using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShopingcartId{ get; set; }
        public int Product_ID { get; set; }
        [ForeignKey("Product_ID")]
        [ValidateNever]
        public Product Product { get; set; }
        [Range(1,100,ErrorMessage ="Count must between 1 to 100")]
        public int Count { get; set; }
        public string UserID{  get; set; }
        [ForeignKey("UserID")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        
        [NotMapped]
        public decimal Price { get; set; }

        


    }
}
