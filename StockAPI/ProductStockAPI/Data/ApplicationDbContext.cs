using Microsoft.EntityFrameworkCore;
using ProductStockAPI.Models; // Importa a classe Product que criamos

namespace ProductStockAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para nossa entidade Product. Isso representa a tabela de produtos no banco de dados.
        public DbSet<Product> Products { get; set; }
    }
}