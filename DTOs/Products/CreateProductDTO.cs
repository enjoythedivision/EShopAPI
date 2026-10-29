using System.ComponentModel.DataAnnotations;
using EshopAPI.Models;

namespace EshopAPI.DTOs
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Artist { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "99999.99")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [EnumDataType(typeof(Product.Category))]
        public Product.Category ProductCategory { get; set; }
    }
}