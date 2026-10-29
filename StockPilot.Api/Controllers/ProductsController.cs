using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Products.Commands;
using StockPilot.Application.Features.Products.DTOs;
using StockPilot.Application.Features.Products.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateProductCommand(request),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetProductByIdQuery(id),cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Product with ID {id} was not found."
                });
            }

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(new GetAllProductsQuery(pageNumber, pageSize, search),cancellationToken);

            return Ok(result);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateProductRequest request,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new UpdateProductCommand(id, request),cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Product with ID {id} was not found."
                });
            }

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteProductCommand(id),cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = $"Product with ID {id} was not found."
                });
            }

            return NoContent();
        }

    }
}
