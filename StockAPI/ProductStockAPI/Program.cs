using Microsoft.EntityFrameworkCore; // Adicione este using
using ProductStockAPI.Data; // Adicione este using para o seu DbContext
using ProductStockAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger funcionar com Minimal APIs
builder.Services.AddSwaggerGen(); // Adiciona os serviços para gerar a documentação Swagger

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita o middleware para servir o documento Swagger gerado
    app.UseSwaggerUI(); // Habilita o middleware para servir a interface do Swagger UI
}

app.UseHttpsRedirection(); // Mantenha isso, mas lembre-se que estamos usando HTTP por enquanto

// Agrupamento de endpoints para Products
var productsApi = app.MapGroup("/api/products");

// GET /api/products - Listar todos os produtos
productsApi.MapGet("/", async (ApplicationDbContext db) =>
    await db.Products.ToListAsync());

// GET /api/products/{id} - Obter um produto específico
productsApi.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product != null ? Results.Ok(product) : Results.NotFound();
});

// POST /api/products - Criar um novo produto
productsApi.MapPost("/", async (Product product, ApplicationDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});

// PUT /api/products/{id} - Atualizar um produto existente
productsApi.MapPut("/{id}", async (int id, Product updatedProduct, ApplicationDbContext db) =>
{
    var existingProduct = await db.Products.FindAsync(id);
    if (existingProduct == null)
    {
        return Results.NotFound();
    }

    // Atualizar as propriedades do produto existente
    existingProduct.Name = updatedProduct.Name;
    existingProduct.Description = updatedProduct.Description;
    existingProduct.Price = updatedProduct.Price;
    existingProduct.Stock = updatedProduct.Stock;
    existingProduct.LastUpdatedDate = DateTime.UtcNow; // Atualiza a data de modificação

    await db.SaveChangesAsync();
    return Results.NoContent(); // Retorna 204 No Content para atualização bem-sucedida
});

// DELETE /api/products/{id} - Deletar um produto
productsApi.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound();
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent(); // Retorna 204 No Content para exclusão bem-sucedida
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}