using Microsoft.AspNetCore.Http.HttpResults;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Endpoints
{
    /// <summary>
    /// Registers HTTP endpoints for product operations on the /products route group.
    /// </summary>
    /// <remarks>
    /// Exposes create, read single, read all, update, and delete operations for products.
    /// </remarks>
    public static class ProductEndpoints
    {
        /// <summary>
        /// Adds the Products route group and handlers.
        /// </summary>
        /// <param name="routes">The endpoint route builder.</param>
        /// <returns>The configured route group builder for /products.</returns>
        public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/products").WithTags("Products");

            group.MapPost("/",
                async (Product product, ProductRepository repository) =>
                {
                    var id = await repository.CreateProductAsync(product);
                    return Results.Created($"/products/{id}", id);
                });

            group.MapGet("/{productId:int}",
                async Task<Results<Ok<Product>, NotFound>> (int productId, ProductRepository repository) =>
                {
                    var product = await repository.GetProductByIdAsync(productId);
                    return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
                });

            group.MapGet("/",
                async (ProductRepository repository) =>
                {
                    var products = await repository.GetAllProductsAsync();
                    return Results.Ok(products);
                });

            group.MapPut("/{productId:int}",
                async (int productId, Product updated, ProductRepository repository) =>
                {
                    var existing = await repository.GetProductByIdAsync(productId);
                    if (existing is null) return Results.NotFound();

                    updated.Id = productId;
                    var ok = await repository.UpdateProductAsync(updated);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/{productId:int}",
                async (int productId, ProductRepository repository) =>
                {
                    var existing = await repository.GetProductByIdAsync(productId);
                    if (existing is null) return Results.NotFound();

                    var ok = await repository.DeleteProductAsync(productId);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            return group;
        }
    }
}