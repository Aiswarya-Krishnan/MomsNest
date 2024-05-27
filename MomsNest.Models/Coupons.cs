using System;
using System.ComponentModel.DataAnnotations;

namespace MomsNest.Models
{
    public class Coupons
    {
        [Key]
        public int CID { get; set; }

        [Required]
        public string Code { get; set; }

        public DiscountType Type { get; set; }

        public double? DiscountAmount { get; set; }
        [Range(1,99,ErrorMessage ="Discount Percentage should be between 1-99")]
        public int? DiscountPercentage { get; set; }
        public bool IsActive { get; set; }

        public int MinimumAmount { get; set; }
        public bool IsEnabled { get; set; }

       
        public enum DiscountType
        {
            DiscountPercentage,
            DiscountAmount
        }
    }
}
