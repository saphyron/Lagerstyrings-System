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
                    var id = await repository.CreateOrderItemAsync(item);
                    return Results.Created($"/orderitems/{id}", id);
                });

            group.MapGet("/by-order/{orderId:long}",
               async (long orderId, OrderItemRepository repository) =>
               {
                   var items = await repository.GetOrderItemsByOrderIdAsync(orderId);
                   return Results.Ok(items);
               });

            group.MapGet("/by-order/{orderId:long}/product/{productId:int}/count",
               async (long orderId, int productId, OrderItemRepository repository) =>
               {
                   var itemCount = await repository.GetOrderItemCountAsync(orderId, productId);
                   return Results.Ok(itemCount ?? 0);
               });

            group.MapPut("/",
                async (OrderItem updatedItem, OrderItemRepository repository) =>
                {
                    var success = await repository.UpdateOrderItemAsync(updatedItem);
                    return success ? Results.NoContent() : Results.StatusCode(500);
                });

            group.MapDelete("/{orderItemId:long}",
                async (long orderItemId, OrderItemRepository repository) =>
                {
                    var success = await repository.DeleteOrderItemAsync(orderItemId);
                    return success ? Results.NoContent() : Results.NotFound();
                });
            return group;
        }
    }
}