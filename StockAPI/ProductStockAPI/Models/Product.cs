using System.ComponentModel.DataAnnotations; // Add this using

namespace ProductStockAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")] // Makes the field required
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")] // Limits the size
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product price is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be a positive value.")] // Price must be greater than zero
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")] // Stock cannot be negative
        public int Stock { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedDate { get; set; }
    }
}