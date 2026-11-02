using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Suppliers.Commands;
using StockPilot.Application.Features.Suppliers.DTOs;
using StockPilot.Application.Features.Suppliers.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSupplier(
            [FromBody] CreateSupplierRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateSupplierCommand(request),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers(
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetAllSuppliersQuery(),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSupplierById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetSupplierByIdQuery(id),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Supplier with ID {id} was not found."
                });
            }

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSupplier(int id,
            [FromBody] UpdateSupplierRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new UpdateSupplierCommand(id, request),cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = $"Supplier with ID {id} was not found."
                });
            }

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSupplier(int id,CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteSupplierCommand(id),cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = $"Supplier with ID {id} was not found."
                });
            }

            return Ok(new
            {
                message = $"Supplier with ID {id} was deleted successfully."
            });
        }
    }
}


