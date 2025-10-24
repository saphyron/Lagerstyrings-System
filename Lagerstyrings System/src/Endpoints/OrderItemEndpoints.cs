using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;

namespace LagerstyringsSystem.Endpoints
{
    public static class OrderItemEndpoints
    {
        public static RouteGroupBuilder MapOrderItemEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/orderitems")
                              .WithTags("OrderItems");

            group.MapPost("/",
                async (OrderItem item, OrderItemRepository repository) =>
                {
                    var orderItemId = await repository.CreateOrderItemAsync(item);
                    return Results.Created($"/orderitems/{orderItemId}", orderItemId);
                });

            group.MapGet("/orders/{orderId}/items",
               async (int orderId, OrderItemRepository repository) =>
               {
                   var items = await repository.GetOrderItemsByOrderIdAsync(orderId);
                   return Results.Ok(items);
               });

            group.MapGet("/orders/{orderId}/items/{productId}/count",
               async (int orderId, int productId, OrderItemRepository repository) =>
               {
                   var itemCount = await repository.GetOrderItemCountAsync(orderId, productId);
                   return Results.Ok(itemCount);
               });

            group.MapPut("/",
                async (OrderItem updatedItem, OrderItemRepository repository) =>
                {
                    var success = await repository.UpdateOrderItemAsync(updatedItem);
                    return success ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/{orderItemId}",
                async (int orderItemId, OrderItemRepository repository) =>
                {
                    var existingItem = await repository.GetOrderItemsByOrderIdAsync(orderItemId);
                    if (existingItem is null)
                    {
                        return Results.NotFound();
                    }
                    var success = await repository.DeleteOrderItemAsync(orderItemId);
                    return success ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/orders/{orderId}/items",
                async (int orderId, OrderItemRepository repository) =>
                {
                    var items = await repository.GetOrderItemsByOrderIdAsync(orderId);
                    if (!items.Any())
                    {
                        return Results.NotFound();
                    }

                    bool allDeleted = true;
                    foreach (var item in items)
                    {
                        var success = await repository.DeleteOrderItemForProductAsync(orderId, item.ProductId);
                        if (!success)
                        {
                            allDeleted = false;
                        }
                    }

                    return allDeleted ? Results.NoContent() : Results.StatusCode(500);
                });
            
            return group;
        }

        // public static void MapOrderItemEndpoints(this WebApplication app)
        // {
        //     app.MapPost("/orderitems", async (OrderItem item, OrderItemRepository repository) =>
        //     {
        //         var orderItemId = await repository.CreateOrderItemAsync(item);
        //         return Results.Created($"/orderitems/{orderItemId}", orderItemId);
        //     });

        //     app.MapGet("/orders/{orderId}/items", async (int orderId, OrderItemRepository repository) =>
        //     {
        //         var items = await repository.GetOrderItemsByOrderIdAsync(orderId);
        //         return Results.Ok(items);
        //     });

        //     app.MapGet("/orders/{orderId}/items/{productId}/count", async (int orderId, int productId, OrderItemRepository repository) =>
        //     {
        //         var itemCount = await repository.GetOrderItemCountAsync(orderId, productId);
        //         return Results.Ok(itemCount);
        //     });

        //     app.MapPut("/orderitems", async (OrderItem updatedItem, OrderItemRepository repository) =>
        //     {
        //         var success = await repository.UpdateOrderItemAsync(updatedItem);
        //         return success ? Results.NoContent() : Results.StatusCode(500);
        //     });

        //     app.MapDelete("/orderitems/{orderItemId}", async (int orderItemId, OrderItemRepository repository) =>
        //     {
        //         var existingItem = await repository.GetOrderItemsByOrderIdAsync(orderItemId);
        //         if (existingItem is null)
        //         {
        //             return Results.NotFound();
        //         }
        //         var success = await repository.DeleteOrderItemAsync(orderItemId);
        //         return success ? Results.NoContent() : Results.StatusCode(500);
        //     });

        //     app.MapDelete("/orders/{orderId}/items", async (int orderId, OrderItemRepository repository) =>
        //     {
        //         var items = await repository.GetOrderItemsByOrderIdAsync(orderId);
        //         if (!items.Any())
        //         {
        //             return Results.NotFound();
        //         }

        //         bool allDeleted = true;
        //         foreach (var item in items)
        //         {
        //             var success = await repository.DeleteOrderItemForProductAsync(orderId, item.ProductId);
        //             if (!success)
        //             {
        //                 allDeleted = false;
        //             }
        //         }

        //         return allDeleted ? Results.NoContent() : Results.StatusCode(500);
        //     });
        // }
    }
}