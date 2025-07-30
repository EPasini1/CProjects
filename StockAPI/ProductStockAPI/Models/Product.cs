namespace ProductStockAPI.Models
{
    public class Product
    {
        public int Id { get; set; } // ID único do produto
        public string Name { get; set; } = string.Empty; // Nome do produto
        public string Description { get; set; } = string.Empty; // Descrição do produto
        public decimal Price { get; set; } // Preço do produto
        public int Stock { get; set; } // Quantidade em estoque
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Data de criação do registro
        public DateTime? LastUpdatedDate { get; set; } // Data da última atualização (opcional)
    }
}