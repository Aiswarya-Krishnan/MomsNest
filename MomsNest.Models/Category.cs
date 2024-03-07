using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MomsNest.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "Maximum length is 40")]
        public string Name { get; set; }

        [DisplayName("Display Order")]

        [Range(1, 100, ErrorMessage = "Display order must be in between 1 - 100")]
        public int DisplayOrder { get; set; }
    }
}
