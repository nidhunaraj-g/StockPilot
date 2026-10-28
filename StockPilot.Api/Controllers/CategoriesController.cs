using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Categories.Commands;
using StockPilot.Application.Features.Categories.DTOs;
using StockPilot.Application.Features.Categories.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateCategoryCommand(request),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
        {
            var categories = await sender.Send(new GetAllCategoriesQuery(),cancellationToken);

            return Ok(categories);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id,CancellationToken cancellationToken)
        {
            var category = await sender.Send(
                new GetCategoryByIdQuery(id),
                cancellationToken);

            if (category is null)
            {
                return NotFound(new
                {
                    message = $"Category with ID {id} was not found."
                });
            }

            return Ok(category);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCategoryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateCategoryCommand(id, request),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Category with ID {id} was not found."
                });
            }

            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id,CancellationToken cancellationToken)
        {
            var deleted = await sender.Send(new DeleteCategoryCommand(id),cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = $"Category with ID {id} was not found."
                });
            }

            return NoContent();
        }


    }
}