using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProductStockAPI.Data; // Certifique-se de que este namespace está correto

namespace ProductStockAPI.Design
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Este é o método que a ferramenta dotnet ef usa para criar uma instância do seu DbContext.

            // Construa a configuração para ler a ConnectionString do appsettings.json.
            // É necessário simular o ambiente de execução da aplicação para obter a ConnectionString.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Obtenha a string de conexão.
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Crie as opções do DbContext usando a string de conexão.
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlite(connectionString);

            // Retorne uma nova instância do seu DbContext com as opções configuradas.
            return new ApplicationDbContext(builder.Options);
        }
    }
}