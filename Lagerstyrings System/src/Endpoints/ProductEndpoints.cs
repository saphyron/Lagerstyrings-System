using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Endpoints
{
    public static class ProductEndpoints
    {
        public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/products").WithTags("Products");

            group.MapPost("/",
            async (Product product, ProductRepository repository) =>
            {
                var productId = await repository.CreateProductAsync(product);
                return Results.Created($"/products/{productId}", productId);
            });

            group.MapGet("/{productId}",
            async Task<Results<Ok<Product>, NotFound>> (int productId, ProductRepository repository) =>
            {
                var product = await repository.GetProductByIdAsync(productId);
                return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
            });

            group.MapGet("/",
            async (ProductRepository repository) =>
            {
                var products = await repository.GetAllProductsAsync();
                return products;
            });

            group.MapPut("/{productId}",
            async (int productId, Product updatedProduct, ProductRepository repository) =>
            {
                var existingProduct = await repository.GetProductByIdAsync(productId);
                if (existingProduct is null)
                {
                    return Results.NotFound();
                }

                updatedProduct.Id = productId;
                var success = await repository.UpdateProductAsync(updatedProduct);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            group.MapDelete("/{productId}",
            async (int productId, ProductRepository repository) =>
            {
                var existingProduct = await repository.GetProductByIdAsync(productId);
                if (existingProduct is null)
                {
                    return Results.NotFound();
                }

                var success = await repository.DeleteProductAsync(productId);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            return group;
        }
    }
}