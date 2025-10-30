using Microsoft.AspNetCore.Http.HttpResults;
using LagerstyringsSystem.Orders;

namespace LagerstyringsSystem.Endpoints
{
    /// <summary>
    /// Registers HTTP endpoints for order operations on the /orders route group.
    /// </summary>
    /// <remarks>
    /// Maps create, read, update, delete, and status patch endpoints.
    /// Uses minimal APIs with dependency-injected repositories.
    /// </remarks>
    public static class OrderEndpoints
    {
        /// <summary>
        /// Adds the Orders route group and its handlers to the endpoint route builder.
        /// </summary>
        /// <param name="routes">The application endpoint route builder.</param>
        /// <returns>The configured route group builder for /orders.</returns>
        /// <remarks>
        /// POST /orders creates an order and returns 201 with the new identifier.
        /// GET /orders/{orderId} returns 200 with the order or 404 if not found.
        /// GET /orders/by-user/{userId} lists orders for a user.
        /// PUT /orders/{orderId} updates a full order and returns 204 or 404.
        /// PATCH /orders/{orderId}/status updates only the order status with validation.
        /// DELETE /orders/{orderId} removes an order and returns 204 or 404.
        /// </remarks>
        public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/orders").WithTags("Orders");

            // POST /orders
            group.MapPost("/",
                async (Order order, OrderRepository repository) =>
                {
                    var orderId = await repository.CreateOrderAsync(order);
                    return Results.Created($"/orders/{orderId}", orderId);
                });

            // GET /orders/{orderId}
            group.MapGet("/{orderId:long}",
                async Task<Results<Ok<Order>, NotFound>> (long orderId, OrderRepository repository) =>
                {
                    var order = await repository.GetOrderByIdAsync(orderId);
                    return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
                });
            //todo: get all orders

            // GET /orders/by-user/{userId}
            group.MapGet("/by-user/{userId:int}",
                async (int userId, OrderRepository repository) =>
                {
                    var orders = await repository.GetAllOrdersByUserAsync(userId);
                    return Results.Ok(orders);
                });

            // PUT /orders/{orderId}
            group.MapPut("/{orderId:long}",
                async (long orderId, Order updatedOrder, OrderRepository repository) =>
                {
                    var existing = await repository.GetOrderByIdAsync(orderId);
                    if (existing is null) return Results.NotFound();

                    updatedOrder.Id = orderId;
                    var ok = await repository.UpdateOrderAsync(updatedOrder);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            // PATCH /orders/{orderId}/status
            group.MapPatch("/{orderId:long}/status",
                async Task<Results<NoContent, NotFound, BadRequest<string>>> (long orderId, StatusRequest body, OrderRepository repository) =>
                {
                    if (string.IsNullOrWhiteSpace(body.Status)) return TypedResults.BadRequest("Status is required.");
                    var allowed = new[] { "Draft", "Shipped", "Cancelled", "OnHold" };
                    if (!allowed.Contains(body.Status)) return TypedResults.BadRequest("Invalid status.");

                    var exists = await repository.GetOrderByIdAsync(orderId);
                    if (exists is null) return TypedResults.NotFound();

                    var ok = await repository.UpdateOrderStatusAsync(orderId, body.Status);
                    return ok ? TypedResults.NoContent() : TypedResults.BadRequest("Could not update status.");
                });

            // DELETE /orders/{orderId}
            group.MapDelete("/{orderId:long}",
                async (long orderId, OrderRepository repository) =>
                {
                    var existing = await repository.GetOrderByIdAsync(orderId);
                    if (existing is null) return Results.NotFound();

                    var ok = await repository.DeleteOrderAsync(orderId);
                    return ok ? Results.NoContent() : Results.StatusCode(500);
                });

            // todo: get all orders endpoint

            return group;
        }
        /// <summary>
        /// Payload for updating an order's status.
        /// </summary>
        /// <remarks>
        /// Accepted values are validated by the endpoint before persistence.
        /// </remarks>
        public sealed class StatusRequest
        {
            /// <summary>
            /// New status value for the order.
            /// </summary>
            /// <value>Allowed values: Draft, Shipped, Cancelled, OnHold.</value>
            public string Status { get; set; } = "";
        }
    }
}
