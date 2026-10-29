using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Brands.Commands;
using StockPilot.Application.Features.Brands.DTOs;
using StockPilot.Application.Features.Brands.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController(ISender sender) : ControllerBase
    {
        // POST: api/Brands
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBrandRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateBrandCommand(request),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        // Temporary endpoint for CreatedAtAction.
        // We will implement the actual query next.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)
        {
            var brand = await sender.Send(new GetBrandByIdQuery(id),cancellationToken);

            if (brand is null)
            {
                return NotFound(new
                {
                    message = $"Brand with ID {id} was not found."
                });
            }

            return Ok(brand);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var brands = await sender.Send(new GetAllBrandsQuery(),cancellationToken);

            return Ok(brands);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id,[FromBody] UpdateBrandRequest request,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new UpdateBrandCommand(id, request),cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Brand with ID {id} was not found."
                });
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteBrandCommand(id),cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = $"Brand with ID {id} was not found."
                });
            }

            return NoContent();
        }
    }
}
