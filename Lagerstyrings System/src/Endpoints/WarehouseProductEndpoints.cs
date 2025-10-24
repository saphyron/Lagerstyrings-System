using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Endpoints
{
    public static class WarehouseProductEndpoints
    {
        public static RouteGroupBuilder MapWarehouseProductEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/warehouseproducts").WithTags("WarehouseProducts");

            group.MapPost("/",
            async (WarehouseProduct warehouseProduct, WarehouseProductRepository repository) =>
            {
                var warehouseProductId = await repository.CreateWarehouseProductAsync(warehouseProduct);
                return Results.Created($"/warehouseproducts/{warehouseProductId}", warehouseProductId);
            });

            group.MapGet("/{warehouseProductId}",
            async Task<Results<Ok<WarehouseProduct>, NotFound>> (int warehouseProductId, WarehouseProductRepository repository) =>
            {
                var warehouseProduct = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                return warehouseProduct is null ? TypedResults.NotFound() : TypedResults.Ok(warehouseProduct);
            });

            group.MapGet("/",
            async (WarehouseProductRepository repository) =>
            {
                var warehouseProducts = await repository.GetAllWarehouseProductsAsync();
                return warehouseProducts;
            });

            group.MapGet("/{warehouseId}/warehouseproducts",
            async (int warehouseId, WarehouseProductRepository repository) =>
            {
                var warehouseProducts = await repository.GetWarehouseProductsByWarehouseIdAsync(warehouseId);
                return warehouseProducts;
            });

            group.MapPut("/{warehouseProductId}",
            async (int warehouseProductId, WarehouseProduct updatedWarehouseProduct, WarehouseProductRepository repository) =>
            {
                var existingWarehouseProduct = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                if (existingWarehouseProduct is null)
                {
                    return Results.NotFound();
                }

                updatedWarehouseProduct.Id = warehouseProductId;
                var success = await repository.UpdateWarehouseProductAsync(updatedWarehouseProduct);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            group.MapDelete("/{warehouseProductId}",
            async (int warehouseProductId, WarehouseProductRepository repository) =>
            {
                var existingWarehouseProduct = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                if (existingWarehouseProduct is null)
                {
                    return Results.NotFound();
                }

                var success = await repository.DeleteWarehouseProductAsync(warehouseProductId);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            return group;
        }
    }
}