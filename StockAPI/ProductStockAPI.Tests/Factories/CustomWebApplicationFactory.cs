// ProductStockAPI.Tests/Factories/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductStockAPI.Data;
using ProductStockAPI;
using ProductStockAPI.DTOs; // For LoginRequest
using System.Net.Http.Json; // For PostAsJsonAsync

namespace ProductStockAPI.Tests.Factories
{
    public class CustomWebApplicationFactory<TMarker> : WebApplicationFactory<TMarker> where TMarker : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite("DataSource=file::memory:?cache=shared");
                });

                var serviceProvider = services.BuildServiceProvider();

                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

                    dbContext.Database.EnsureCreated();

                    // Seed a test user for authentication
                    // You might need to adjust this based on your Identity setup
                    if (!dbContext.Users.Any())
                    {
                        var userManager = scopedServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
                        var user = new Microsoft.AspNetCore.Identity.IdentityUser { UserName = "testuser", Email = "test@example.com" };
                        var result = userManager.CreateAsync(user, "Test@123").Result; // Create user with password
                        if (!result.Succeeded)
                        {
                            throw new Exception("Failed to seed test user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }
            });
        }

        // Method to create an HttpClient with an authenticated user
        public async Task<HttpClient> CreateClientWithAuthenticatedUserAsync()
        {
            var client = CreateClient();

            // Perform login to get a valid token
            var loginRequest = new LoginRequest("test@example.com", "Test@123"); // Assuming LoginRequest DTO exists
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            loginResponse.EnsureSuccessStatusCode();
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(); // Assuming LoginResponse DTO exists

            Assert.NotNull(loginResult?.Token); // Ensure token is not null

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

            return client;
        }
    }
}