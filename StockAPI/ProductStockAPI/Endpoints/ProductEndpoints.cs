// File: Endpoints/ProductEndpoints.cs

using Microsoft.EntityFrameworkCore;
using ProductStockAPI.Data;
using ProductStockAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductStockAPI.Endpoints;

public static class ProductEndpoints
{
    // Add this helper method inside the ProductEndpoints class
    private static IResult? ValidateProduct(Product product)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(product);

        if (!Validator.TryValidateObject(product, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults.ToDictionary(
                v => v.MemberNames.FirstOrDefault() ?? "General",
                v => new string[] { v.ErrorMessage ?? "Unknown validation error." }
            );
            return Results.ValidationProblem(errors);
        }
        return null; // Indicates validation passed
    }
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var productsApi = app.MapGroup("/api/products");

        // GET /api/products - List all products
        productsApi.MapGet("/", async (ApplicationDbContext db) =>
            await db.Products.ToListAsync());

        // GET /api/products/{id} - Get a specific product
        productsApi.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return Results.NotFound(new { message = $"Product with ID '{id}' was not found." });
            }
            return Results.Ok(product);
        });

        // POST /api/products - Create a new product (REQUIRES AUTHORIZATION)
        productsApi.MapPost("/", async (Product product, ApplicationDbContext db) =>
        {
            var validationResult = ValidateProduct(product);
            if (validationResult != null) return validationResult; // Return validation errors if any

            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{product.Id}", product);
        }).RequireAuthorization();

        // PUT /api/products/{id} - Update an existing product (REQUIRES AUTHORIZATION)
        productsApi.MapPut("/{id}", async (int id, Product updatedProduct, ApplicationDbContext db) =>
        {
            var validationResult = ValidateProduct(updatedProduct);
            if (validationResult != null) return validationResult; // Return validation errors if any

            var existingProduct = await db.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return Results.NotFound(new { message = $"Product with ID '{id}' was not found." });
            }

            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Stock = updatedProduct.Stock;
            existingProduct.LastUpdatedDate = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        // DELETE /api/products/{id} - Delete a product (REQUIRES AUTHORIZATION)
        productsApi.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return Results.NotFound(new { message = $"Product with ID '{id}' was not found." });
            }

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
