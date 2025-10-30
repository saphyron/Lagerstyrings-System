using Microsoft.AspNetCore.Http.HttpResults;
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
                    var id = await repository.CreateWarehouseProductAsync(warehouseProduct);
                    return Results.Created($"/warehouseproducts/{id}", id);
                });

            group.MapGet("/{warehouseProductId:int}",
                async Task<Results<Ok<WarehouseProduct>, NotFound>> (int warehouseProductId, WarehouseProductRepository repository) =>
                {
                    var wp = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                    return wp is null ? TypedResults.NotFound() : TypedResults.Ok(wp);
                });

            group.MapGet("/",
                async (WarehouseProductRepository repository) =>
                {
                    var items = await repository.GetAllWarehouseProductsAsync();
                    return Results.Ok(items);
                });

            group.MapGet("/by-warehouse/{warehouseId:int}",
                async (int warehouseId, WarehouseProductRepository repository) =>
                {
                    var items = await repository.GetWarehouseProductsByWarehouseIdAsync(warehouseId);
                    return Results.Ok(items);
                });

            group.MapPut("/{warehouseProductId:int}",
                async (int warehouseProductId, WarehouseProduct updated, WarehouseProductRepository repository) =>
                {
                    var existing = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                    if (existing is null) return Results.NotFound();

                    updated.Id = warehouseProductId;
                    var ok = await repository.UpdateWarehouseProductAsync(updated);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/{warehouseProductId:int}",
                async (int warehouseProductId, WarehouseProductRepository repository) =>
                {
                    var existing = await repository.GetWarehouseProductByIdAsync(warehouseProductId);
                    if (existing is null) return Results.NotFound();

                    var ok = await repository.DeleteWarehouseProductAsync(warehouseProductId);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            return group;
        }
    }
}