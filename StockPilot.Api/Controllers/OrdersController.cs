using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Features.Orders.Commands;
using StockPilot.Application.Features.Orders.DTOs;
using StockPilot.Application.Features.Orders.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(ISender sender) : ControllerBase
    {
        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new CreateOrderCommand(request),
                    cancellationToken);

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = result.Id },
                    result);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "Product stock was changed by another request. Please refresh the stock and try again."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(
                    new GetAllOrdersQuery(
                        pageNumber,
                        pageSize,
                        search,
                        status),
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new GetOrderByIdQuery(id),
                    cancellationToken);

                if (result is null)
                {
                    return NotFound(new
                    {
                        message = $"Order with ID {id} was not found."
                    });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                await sender.Send(
                    new CancelOrderCommand(id),
                    cancellationToken);

                return Ok(new
                {
                    message = $"Order {id} was cancelled successfully. Stock has been restored."
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "The order or product stock was changed by another request. Refresh the order and try again."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}