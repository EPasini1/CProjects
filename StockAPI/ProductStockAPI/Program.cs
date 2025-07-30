// File: Program.cs (Cleaned Version)

// Add using statements for our new extension methods
using ProductStockAPI.Endpoints;
using ProductStockAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add all services to the container using our single extension method
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// 2. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 3. Map all endpoints using our extension methods
app.MapAuthEndpoints();
app.MapProductEndpoints();

// (Optional) Keep any other non-grouped endpoints here
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            "Some Summary"
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


app.Run();

// Keep record types here or move them to a 'DTOs' or 'Models' folder for even more organization
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}