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
    public class OrderDetails
    {
        public int OrderDetailsId   { get; set; }
        [Required] 
        public int OrderHeader_ID { get; set; }
        [ForeignKey("OrderHeader_ID")]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public int Product_ID { get; set; }
        [ForeignKey("Product_ID")]
        [ValidateNever]

        public Product Product { get; set; }

        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
