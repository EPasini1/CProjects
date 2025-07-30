// File: Endpoints/ProductEndpoints.cs

using Microsoft.EntityFrameworkCore;
using ProductStockAPI.Data;
using ProductStockAPI.Models;
using System.ComponentModel.DataAnnotations; // Still needed for models, but not for DTO validation check here
using ProductStockAPI.DTOs; // Import your DTOs
using Microsoft.AspNetCore.Http; // For StatusCodes

namespace ProductStockAPI.Endpoints;

public static class ProductEndpoints
{
    // O método ValidateProduct() não é mais necessário aqui para validação de entrada,
    // pois as Minimal APIs validam automaticamente os DTOs nos parâmetros.
    // Deixá-lo como estava seria para validação manual de um Product model,
    // mas o ideal é que a validação esteja nos DTOs de entrada.
    // Se você mantiver o método, ele não será mais chamado aqui.
    // Se quiser reutilizar a lógica de validação de 'Product', pode manter o método, mas a chamada será diferente.
    // Para simplificar, vou remover a chamada explícita de ValidateProduct nos endpoints.

    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productsApi = app.MapGroup("/api/products");

        // GET /api/products - List all products
        // Return a list of ProductResponse DTOs
        productsApi.MapGet("/", async (ApplicationDbContext db) =>
            await db.Products.Select(p => ProductResponse.FromProduct(p)).ToListAsync());

        // GET /api/products/{id} - Get a specific product
        // Return a ProductResponse DTO
        productsApi.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                // Corrected to use Problem Details format for 404
                return Results.NotFound(new
                {
                    type = "https://yourapi.com/errors/not-found",
                    title = "Product Not Found",
                    status = StatusCodes.Status404NotFound,
                    detail = $"Product with ID '{id}' was not found.",
                    instance = $"/api/products/{id}"
                });
            }
            return Results.Ok(ProductResponse.FromProduct(product)); // Return ProductResponse
        });

        // POST /api/products - Create a new product (REQUIRES AUTHORIZATION)
        // Use CreateProductRequest as the input parameter
        productsApi.MapPost("/", async (CreateProductRequest request, ApplicationDbContext db) =>
        {
            // Minimal APIs automatically handle validation on 'request' based on DTO attributes.
            // If validation fails, it will return Results.ValidationProblem(errors) automatically.
            // No need for explicit 'ValidateProduct' call here for DTOs.

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CreatedDate = DateTime.UtcNow // Server-generated
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{product.Id}", ProductResponse.FromProduct(product)); // Return ProductResponse
        }).RequireAuthorization();

        // PUT /api/products/{id} - Update an existing product (REQUIRES AUTHORIZATION)
        // Use UpdateProductRequest as the input parameter
        productsApi.MapPut("/{id}", async (int id, UpdateProductRequest request, ApplicationDbContext db) =>
        {
            // Minimal APIs automatically handle validation on 'request' based on DTO attributes.
            // If validation fails, it will return Results.ValidationProblem(errors) automatically.
            // No need for explicit 'ValidateProduct' call here for DTOs.

            var existingProduct = await db.Products.FindAsync(id);
            if (existingProduct == null)
            {
                // Corrected to use Problem Details format for 404
                return Results.NotFound(new
                {
                    type = "https://yourapi.com/errors/not-found",
                    title = "Product Not Found",
                    status = StatusCodes.Status404NotFound,
                    detail = $"Product with ID '{id}' was not found.",
                    instance = $"/api/products/{id}"
                });
            }

            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.Stock = request.Stock;
            existingProduct.LastUpdatedDate = DateTime.UtcNow; // Server-generated/updated

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // DELETE /api/products/{id} - Delete a product (REQUIRES AUTHORIZATION)
        productsApi.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                // Corrected to use Problem Details format for 404
                return Results.NotFound(new
                {
                    type = "https://yourapi.com/errors/not-found",
                    title = "Product Not Found",
                    status = StatusCodes.Status404NotFound,
                    detail = $"Product with ID '{id}' could not be deleted because it was not found.",
                    instance = $"/api/products/{id}"
                });
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}