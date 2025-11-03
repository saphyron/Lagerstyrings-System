using Microsoft.AspNetCore.Http.HttpResults;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Endpoints
{
    public static class WarehouseEndpoints
    {
        public static RouteGroupBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/warehouses").WithTags("Warehouses");

            group.MapPost("/",
                async (Warehouse warehouse, WarehouseRepository repository) =>
                {
                    var id = await repository.CreateWarehouseAsync(warehouse);
                    return Results.Created($"/warehouses/{id}", id);
                });

            group.MapGet("/{warehouseId:int}",
                async Task<Results<Ok<Warehouse>, NotFound>> (int warehouseId, WarehouseRepository repository) =>
                {
                    var warehouse = await repository.GetWarehouseByIdAsync(warehouseId);
                    return warehouse is null ? TypedResults.NotFound() : TypedResults.Ok(warehouse);
                });

            group.MapGet("/",
                async (WarehouseRepository repository) =>
                {
                    var warehouses = await repository.GetAllWarehousesAsync();
                    return Results.Ok(warehouses);
                });

            group.MapPut("/{warehouseId:int}",
                async (int warehouseId, Warehouse updated, WarehouseRepository repository) =>
                {
                    var existing = await repository.GetWarehouseByIdAsync(warehouseId);
                    if (existing is null) return Results.NotFound();

                    updated.Id = warehouseId;
                    var ok = await repository.UpdateWarehouseAsync(updated);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/{warehouseId:int}",
                async (int warehouseId, WarehouseRepository repository) =>
                {
                    var existing = await repository.GetWarehouseByIdAsync(warehouseId);
                    if (existing is null) return Results.NotFound();

                    var ok = await repository.DeleteWarehouseAsync(warehouseId);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            return group;
        }
    }
}