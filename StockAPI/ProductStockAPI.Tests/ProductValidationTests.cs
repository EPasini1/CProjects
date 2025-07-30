using ProductStockAPI.Models; // Para acessar o modelo Product
using System.ComponentModel.DataAnnotations; // Para ValidationContext e Validator
using Xunit; // Para atributos de teste como [Fact] e Assert
using System.Collections.Generic; // Para List<ValidationResult>

namespace ProductStockAPI.Tests
{
    public class ProductValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
            return validationResults;
        }

        [Fact] // Este atributo marca o método como um método de teste
        public void Product_Should_Be_Valid_With_Correct_Data()
        {
            // Arrange (Organizar): Prepare os dados de entrada
            var product = new Product
            {
                Name = "Example Product",
                Description = "A valid product description.",
                Price = 10.00m,
                Stock = 5
            };

            // Act (Agir): Execute a ação que você quer testar
            var results = ValidateModel(product);

            // Assert (Verificar): Verifique se o resultado é o esperado
            Assert.Empty(results); // Espera-se que não haja erros de validação
        }

        [Fact]
        public void Product_Should_Be_Invalid_When_Name_Is_Missing()
        {
            // Arrange
            var product = new Product
            {
                Name = "", // Nome ausente
                Description = "A valid description",
                Price = 10.00m,
                Stock = 5
            };

            // Act
            var results = ValidateModel(product);

            // Assert
            Assert.NotEmpty(results); // Espera-se que haja erros de validação
             Assert.Contains(results, r => r.MemberNames.Contains("Name") && r.ErrorMessage!.Contains("Product name is required."));
        }

        [Fact]
        public void Product_Should_Be_Invalid_When_Price_Is_Negative()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Description",
                Price = -5.00m, // Preço negativo
                Stock = 10
            };

            // Act
            var results = ValidateModel(product);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Price") && r.ErrorMessage!.Contains("Price must be a positive value."));
        }

        // Você pode adicionar mais testes aqui para cobrir outras regras:
        // - Nome muito longo
        // - Estoque negativo
        // - etc.
    }
}