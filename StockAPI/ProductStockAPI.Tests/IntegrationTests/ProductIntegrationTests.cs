// ProductStockAPI.Tests/IntegrationTests/ProductIntegrationTests.cs
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProductStockAPI.Tests.Factories;
using ProductStockAPI;
using ProductStockAPI.DTOs;

namespace ProductStockAPI.Tests.IntegrationTests
{
    public class ProductIntegrationTests : IClassFixture<CustomWebApplicationFactory<ApiMarker>>
    {
        private readonly CustomWebApplicationFactory<ApiMarker> _factory;
        private HttpClient _authenticatedClient; // Now this will be assigned with a token

        public ProductIntegrationTests(CustomWebApplicationFactory<ApiMarker> factory)
        {
            _factory = factory;
        }

        // XUnit provides a way to run async setup before tests in a fixture or collection.
        // For per-test setup, you can use IAsyncLifetime or put it in the constructor
        // but for HttpClient, it's better to do it once if possible or
        // create a new client for each test if isolation is critical.
        // For simplicity, let's make the setup for the authenticated client in a setup method
        // that gets called before each test that requires it.

        public async Task InitializeAsync()
        {
            // This method will be called before each test in this fixture
            _authenticatedClient = await _factory.CreateClientWithAuthenticatedUserAsync();
        }

        public async Task DisposeAsync()
        {
            _authenticatedClient?.Dispose();
            // Clean up data if necessary
        }


        // Test for POST /api/products
        [Fact]
        public async Task Post_Product_ReturnsCreatedStatusAndProductResponse()
        {
            await InitializeAsync(); // Call setup for authenticated client

            // Arrange
            var request = new CreateProductRequest
            {
                Name = "Integration Test Product",
                Description = "Description for integration test.",
                Price = 99.99m,
                Stock = 10
            };

            // Act
            var response = await _authenticatedClient.PostAsJsonAsync("/api/products", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

            var productResponse = await response.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(productResponse);
            Assert.True(productResponse.Id > 0);
            Assert.Equal(request.Name, productResponse.Name);
            Assert.Equal(request.Description, productResponse.Description);
            Assert.Equal(request.Price, productResponse.Price);
            Assert.Equal(request.Stock, productResponse.Stock);

            await DisposeAsync(); // Clean up
        }

        // Test for POST /api/products with invalid data
        [Fact]
        public async Task Post_Product_WithInvalidData_ReturnsBadRequest()
        {
            await InitializeAsync(); // Call setup for authenticated client

            // Arrange
            var request = new CreateProductRequest
            {
                Name = "", // Invalid: Empty name
                Description = "Description for invalid test.",
                Price = 99.99m,
                Stock = 10
            };

            // Act
            var response = await _authenticatedClient.PostAsJsonAsync("/api/products", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            await DisposeAsync(); // Clean up
        }

        // You'll also want tests for unauthorized access (401)
        [Fact]
        public async Task Post_Product_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var unauthenticatedClient = _factory.CreateClient(); // A client without a token
            var request = new CreateProductRequest
            {
                Name = "Unauthorized Test Product",
                Description = "Description for unauthorized test.",
                Price = 1.00m,
                Stock = 1
            };

            // Act
            var response = await unauthenticatedClient.PostAsJsonAsync("/api/products", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}