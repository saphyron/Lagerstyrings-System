using System.Text;
using Dapper;
using LagerstyringsSystem.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;

namespace LagerstyringsSystem.Endpoints
{
    public static class OrderEndpoints
    {
        public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/orders")
                              .WithTags("Orders");

            // POST /orders
            group.MapPost("/",
                async (Order order, OrderRepository repository) =>
                {
                    var orderId = await repository.CreateOrderAsync(order);
                    return Results.Created($"/orders/{orderId}", orderId);
                });

            // GET /orders/{orderId}
            group.MapGet("/{orderId}",
                async Task<Results<Ok<Order>, NotFound>> (int orderId, OrderRepository repository) =>
                {
                    var order = await repository.GetOrderByIdAsync(orderId);
                    return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
                });

            // GET /auth/users/{userId}/orders
            group.MapGet("/auth/users/{userId}/orders",
                async (int userId, OrderRepository repository) =>
                {
                    var orders = await repository.GetAllOrdersByUserAsync(userId);
                    return Results.Ok(orders);
                });

            // PUT /orders/{orderId}
            group.MapPut("/{orderId}",
                async (int orderId, Order updatedOrder, OrderRepository repository) =>
                {
                    var existingOrder = await repository.GetOrderByIdAsync(orderId);
                    if (existingOrder is null)
                    {
                        return Results.NotFound();
                    }

                    updatedOrder.Id = orderId;
                    var success = await repository.UpdateOrderAsync(updatedOrder);
                    return success ? Results.NoContent() : Results.StatusCode(500);
                });

            // DELETE /orders/{orderId}
            group.MapDelete("/{orderId}",
                async (int orderId, OrderRepository repository) =>
                {
                    var existingOrder = await repository.GetOrderByIdAsync(orderId);
                    if (existingOrder is null)
                    {
                        return Results.NotFound();
                    }

                    var success = await repository.DeleteOrderAsync(orderId);
                    return success ? Results.NoContent() : Results.StatusCode(500);
                });

            return group;
        }

        // public static void MapOrderEndpoints(this WebApplication app)
        // {
        //     app.MapPost("/orders", async (Order order, OrderRepository repository) =>
        //     {
        //         var orderId = await repository.CreateOrderAsync(order);
        //         return Results.Created($"/orders/{orderId}", orderId);
        //     });

        //     app.MapGet("/orders/{orderId}", async (int orderId, OrderRepository repository) =>
        //     {
        //         var order = await repository.GetOrderByIdAsync(orderId);
        //         return order is not null ? Results.Ok(order) : Results.NotFound();
        //     });

        //     app.MapGet("/auth/users/{userId}/orders", async (int userId, OrderRepository repository) =>
        //     {
        //         var orders = await repository.GetAllOrdersByUserAsync(userId);
        //         return Results.Ok(orders);
        //     });

        //     app.MapPut("/orders/{orderId}", async (int orderId, Order updatedOrder, OrderRepository repository) =>
        //     {
        //         var existingOrder = await repository.GetOrderByIdAsync(orderId);
        //         if (existingOrder is null)
        //         {
        //             return Results.NotFound();
        //         }

        //         updatedOrder.Id = orderId;
        //         var success = await repository.UpdateOrderAsync(updatedOrder);
        //         return success ? Results.NoContent() : Results.StatusCode(500);
        //     });

        //     app.MapDelete("/orders/{orderId}", async (int orderId, OrderRepository repository) =>
        //     {
        //         var existingOrder = await repository.GetOrderByIdAsync(orderId);
        //         if (existingOrder is null)
        //         {
        //             return Results.NotFound();
        //         }

        //         var success = await repository.DeleteOrderAsync(orderId);
        //         return success ? Results.NoContent() : Results.StatusCode(500);
        //     });
        // }
    }
}