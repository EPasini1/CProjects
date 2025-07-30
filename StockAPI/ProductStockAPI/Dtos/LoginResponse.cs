// ProductStockAPI/DTOs/LoginResponse.cs
namespace ProductStockAPI.DTOs
{
    public record LoginResponse(string Token, DateTime Expiration);
}