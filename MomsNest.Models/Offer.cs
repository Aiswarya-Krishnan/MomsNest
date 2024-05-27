using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomsNest.Models
{
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }

        [Required]
        public string OfferName { get; set; }

        public OfferType Offertype { get; set; }

        public string? OfferItem { get; set; }

        public double OfferDiscount { get; set; }

        public string OfferDescription { get; set; }

        public enum OfferType
        {
            Category,
            Product
        }
    }
}
