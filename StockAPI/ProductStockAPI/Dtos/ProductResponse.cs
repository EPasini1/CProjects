// ProductStockAPI/DTOs/ProductResponse.cs
namespace ProductStockAPI.DTOs
{
    public record ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        // Método de factory para converter de Product para ProductResponse
        public static ProductResponse FromProduct(Models.Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CreatedDate = product.CreatedDate,
                LastUpdatedDate = product.LastUpdatedDate
            };
        }
    }
}