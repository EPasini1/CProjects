using Microsoft.EntityFrameworkCore;
using ProductStockAPI.Data;
using ProductStockAPI.Models;
using System.ComponentModel.DataAnnotations; // ADICIONE/CONFIRME ESTE
using System.Linq; // ADICIONE/CONFIRME ESTE
using Microsoft.AspNetCore.Http; // Para StatusCodes

// Usings para Autenticação e Autorização (Módulo 6, caso tenhamos chegado a essas mudanças)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // Para JwtSecurityTokenHandler
using Microsoft.OpenApi.Models; // Para Swagger JWT config

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Necessário para o Swagger funcionar com Minimal APIs
builder.Services.AddSwaggerGen(c =>
{
    // Configuration to add the authorization option (Bearer Token) in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,

            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings (examples, adjust for your security needs)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true; // Ensures emails are unique
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Uses EF Core to store identity data
.AddDefaultTokenProviders(); // For generating tokens for password reset, etc.

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey not found."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true, // Validate the token's issuer
        ValidIssuer = jwtSettings["Issuer"], // The token's issuer (your API)
        ValidateAudience = true, // Validate the token's audience
        ValidAudience = jwtSettings["Audience"], // The token's audience (who consumes the token)
        ValidateLifetime = true, // Validate the token's lifetime
        ClockSkew = TimeSpan.Zero // No tolerance for expiration
    };
});

builder.Services.AddAuthorization(); // Enable the authorization service

var app = builder.Build();

// NEW: Add authentication and authorization middlewares to the request pipeline
app.UseAuthentication();
app.UseAuthorization();

// Authentication Endpoints Group
var authApi = app.MapGroup("/api/auth");

// Register Endpoint
authApi.MapPost("/register", async (RegisterRequest request, UserManager<IdentityUser> userManager) =>
{
    var user = new IdentityUser { UserName = request.Email, Email = request.Email };
    var result = await userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
        return Results.Ok(new { Message = "User registered successfully!" });
    }
    return Results.BadRequest(result.Errors);
});

// Login Endpoint
authApi.MapPost("/login", async (LoginRequest request, UserManager<IdentityUser> userManager, IConfiguration configuration) =>
{
    var user = await userManager.FindByEmailAsync(request.Email);
    if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
    {
        return Results.Unauthorized();
    }

    // Generate JWT
    var jwtSettings = configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!)
        }),
        Expires = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryMinutes")),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"]
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwtToken = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = jwtToken });
});

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

// GET /api/products/{id} - Get a specific product (can be public)
productsApi.MapGet("/{id}", async (int id, ApplicationDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound(new { type = "https://yourapi.com/errors/not-found", title = "Product Not Found", status = StatusCodes.Status404NotFound, detail = $"Product with ID '{id}' was not found.", instance = $"/api/products/{id}" });
    }
    return Results.Ok(product);
});

// POST /api/products - Create a new product (REQUIRES AUTHORIZATION)
productsApi.MapPost("/", async (Product product, ApplicationDbContext db) =>
{
    // ... (existing validation code) ...
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
}).RequireAuthorization(); // <--- NEW HERE

// PUT /api/products/{id} - Update an existing product (REQUIRES AUTHORIZATION)
productsApi.MapPut("/{id}", async (int id, Product updatedProduct, ApplicationDbContext db) =>
{
    // ... (existing validation code) ...
    // ... (existing update logic) ...
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // <--- NEW HERE

// DELETE /api/products/{id} - Delete a product (REQUIRES AUTHORIZATION)
productsApi.MapDelete("/{id}", async (int id, ApplicationDbContext db) =>
{
    // ... (existing delete logic) ...
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(); // <--- NEW HERE

app.Run();
// NEW: Define record classes for RegisterRequest and LoginRequest
record RegisterRequest(string Email, string Password);
record LoginRequest(string Email, string Password);