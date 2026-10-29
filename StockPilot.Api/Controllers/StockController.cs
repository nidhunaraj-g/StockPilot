using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Stocks.Commands;
using StockPilot.Application.Features.Stocks.DTOs;
using StockPilot.Application.Features.Stocks.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController(ISender sender) : ControllerBase
    {
        [HttpPost("adjust")]
        public async Task<ActionResult<StockMovementResponse>> AdjustStock([FromBody] AdjustStockRequest request,CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new AdjustStockCommand(request),
                    cancellationToken);

                return Ok(result);
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


        [HttpGet("movements")]
        public async Task<ActionResult<StockMovementListResponse>> GetMovements(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? productId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(
                    new GetStockMovementsQuery(pageNumber, pageSize, productId),
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<LowStockProductListResponse>> GetLowStockProducts([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(new GetLowStockProductsQuery(pageNumber, pageSize),cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<StockSummaryResponse>> GetStockSummary(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetStockSummaryQuery(),cancellationToken);

            return Ok(result);
        }
    }
}
