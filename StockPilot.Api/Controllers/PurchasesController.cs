using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockPilot.Application.Features.Purchases.Commands;
using StockPilot.Application.Features.Purchases.DTOs;
using StockPilot.Application.Features.Purchases.Queries;
using System.ComponentModel.DataAnnotations;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController(ISender sender) : ControllerBase
    {
        // POST: api/Purchases
        [HttpPost]
        public async Task<IActionResult> CreatePurchase(
            [FromBody] CreatePurchaseRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new CreatePurchaseCommand(request),
                    cancellationToken);

                return CreatedAtAction(
                    nameof(GetPurchaseById),
                    new { id = result.Id },
                    result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "The product or supplier data was changed by another request. Please try again."
                });
            }
        }

        // GET: api/Purchases
        [HttpGet]
        public async Task<IActionResult> GetAllPurchases(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(
                    new GetAllPurchasesQuery(
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

        // GET: api/Purchases/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPurchaseById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetPurchaseByIdQuery(id),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Purchase with ID {id} was not found."
                });
            }

            return Ok(result);
        }

        // PUT: api/Purchases/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePurchase(
            int id,
            [FromBody] CreatePurchaseRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new UpdatePurchaseCommand(id, request),
                    cancellationToken);

                if (result is null)
                {
                    return NotFound(new
                    {
                        message = $"Purchase with ID {id} was not found."
                    });
                }

                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "Purchase data was changed by another request. Please refresh and try again."
                });
            }
        }

        // PUT: api/Purchases/5/confirm
        [HttpPut("{id:int}/confirm")]
        public async Task<IActionResult> ConfirmPurchase(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new ConfirmPurchaseCommand(id),
                    cancellationToken);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Purchase with ID {id} was not found."
                    });
                }

                return Ok(new
                {
                    message = $"Purchase {id} was confirmed successfully. Stock has not been changed."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Purchases/5/receive
        [HttpPut("{id:int}/receive")]
        public async Task<IActionResult> ReceivePurchase(
            int id,
            [FromBody] ReceivePurchaseRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new ReceivePurchaseCommand(id, request),
                    cancellationToken);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Purchase with ID {id} was not found."
                    });
                }

                return Ok(new
                {
                    message = $"Products for purchase {id} were received successfully. Stock has been updated."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "Product stock was changed by another request. Refresh the stock and try again."
                });
            }
        }

        // PUT: api/Purchases/5/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelPurchase(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new CancelPurchaseCommand(id),
                    cancellationToken);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Purchase with ID {id} was not found."
                    });
                }

                return Ok(new
                {
                    message = $"Purchase {id} was cancelled successfully."
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
