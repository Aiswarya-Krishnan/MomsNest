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
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "Maximum length is 40")]
        public string ProductName { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        [Range(1,25000)]
        public decimal Price { get; set; }
        [Required]
        [Range(0, 200)]
        [Display(Name ="Stock Quantity")]
        public int StockQuantity { get; set; }
        [Required]
        [MaxLength(40, ErrorMessage = "Maximum length is 40")]
        public string Brand { get; set; }
        [Required]
        public string Color { get; set; }
        [Required]
        public string Material { get; set; }
        [Required]
        public decimal Weight { get; set; }

        public int CategoryID{  get; set; }
        [ForeignKey("CategoryID")]
        [ValidateNever]
        public Category Category{ get; set; }
        [ValidateNever]

        [Required(ErrorMessage = "At least one product image is required")]
        public List<ProductImage> ProductImages { get; set; }


        public double? Discount { get; set; }




    }
}
