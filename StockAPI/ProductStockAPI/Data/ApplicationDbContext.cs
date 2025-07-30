using Microsoft.EntityFrameworkCore;
using ProductStockAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Add this using
using Microsoft.AspNetCore.Identity; // Add this using for IdentityUser

namespace ProductStockAPI.Data
{
    // NEW: Inherit from IdentityDbContext<IdentityUser>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> // <--- MODIFY HERE
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        // Optional: If you need to configure identity model properties (not doing it now, but good to know)
        // protected override void OnModelCreating(ModelBuilder builder)
        // {
        //     base.OnModelCreating(builder);
        //     // Your custom model configurations here
        // }
    }
}