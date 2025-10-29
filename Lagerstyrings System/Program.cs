using LagerstyringsSystem.Database;
using LagerstyringsSystem.Endpoints.AuthenticationEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LagerstyringsSystem.Endpoints;
using LagerstyringsSystem.Orders;
using Lagerstyrings_System;

var builder = WebApplication.CreateBuilder(args);

// Connection factory
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<WarehouseRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<WarehouseProductRepository>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LSS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;

/*static string KeyFingerprint(string key)
{
    using var sha = System.Security.Cryptography.SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(key);
    var hash = sha.ComputeHash(bytes);
    return Convert.ToHexString(hash); // f.eks. "A1B2C3..."
}*/ // Dev: brugbar til debugging af nøgler

//Console.WriteLine($"JWT Key SHA256 (backend):  {KeyFingerprint(jwtKey)}"); // dev debugging


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapUserEndpoints();
app.MapAuthEndpoints();
app.MapOrderEndpoints();
app.MapOrderItemEndpoints();
app.MapWarehouseEndpoints();
app.MapProductEndpoints();
app.MapWarehouseProductEndpoints();

app.MapGet("/", () => Results.Ok(new { ok = true, service = "Lagerstyrings System API" }));

app.Run();
