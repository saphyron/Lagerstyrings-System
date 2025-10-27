using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;
using Lagerstyrings_System;

namespace LagerstyringsSystem.Endpoints
{
    public static class WarehouseEndpoints
    {
        public static RouteGroupBuilder MapWarehouseEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/warehouses").WithTags("Warehouses");

            //POST /warehouses
            group.MapPost("/",
            async (Warehouse warehouse, WarehouseRepository repository) =>
            {
                var warehouseId = await repository.CreateWarehouseAsync(warehouse);
                return Results.Created($"/warehouses/{warehouseId}", warehouseId);
            });

            group.MapGet("/{warehouseId}",
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

            group.MapPut("/{warehouseId}",
            async (int warehouseId, Warehouse updatedWarehouse, WarehouseRepository repository) =>
            {
                var existingWarehouse = await repository.GetWarehouseByIdAsync(warehouseId);
                if (existingWarehouse is null)
                {
                    return Results.NotFound();
                }

                updatedWarehouse.Id = warehouseId;
                var success = await repository.UpdateWarehouseAsync(updatedWarehouse);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            group.MapDelete("/{warehouseId}",
            async (int warehouseId, WarehouseRepository repository) =>
            {
                var existingWarehouse = await repository.GetWarehouseByIdAsync(warehouseId);
                if (existingWarehouse is null)
                {
                    return Results.NotFound();
                }

                var success = await repository.DeleteWarehouseAsync(warehouseId);
                return success ? Results.NoContent() : Results.StatusCode(500);
            });

            return group;
        }
    }
}