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

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LSS";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? jwtIssuer;

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

app.MapGet("/", () => Results.Ok(new { ok = true, service = "Lagerstyrings System API" }));

app.Run();
