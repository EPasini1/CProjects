// ProductStockAPI/DTOs/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace ProductStockAPI.DTOs
{
    public record LoginRequest(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password
    );
}