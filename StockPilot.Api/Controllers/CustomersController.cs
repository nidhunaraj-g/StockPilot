
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Customers.Commands;
using StockPilot.Application.Features.Customers.DTOs;
using StockPilot.Application.Features.Customers.Queries;

namespace StockPilot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(ISender sender) : ControllerBase
    {
        // PUBLIC: Customer registration
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            CustomerRegisterRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new RegisterCustomerCommand(request),
                    cancellationToken);

                return Ok(result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUBLIC: Customer login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            CustomerLoginRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new CustomerLoginCommand(request),
                    cancellationToken);

                return Ok(result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // CUSTOMER: Get own profile
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile(
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerIdentity(
                out var userId,
                out var customerId))
            {
                return Forbid();
            }

            var result = await sender.Send(
                new GetMyCustomerProfileQuery(userId, customerId),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = "Customer profile not found."
                });
            }

            return Ok(result);
        }

        // CUSTOMER: Update own profile
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateMyCustomerRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerIdentity(
                out var userId,
                out var customerId))
            {
                return Forbid();
            }

            try
            {
                var result = await sender.Send(
                    new UpdateMyCustomerProfileCommand(
                        userId,
                        customerId,
                        request),
                    cancellationToken);

                if (result is null)
                {
                    return NotFound(new
                    {
                        message = "Customer profile not found."
                    });
                }

                return Ok(result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // CUSTOMER: Deactivate own account
        [Authorize]
        [HttpPut("me/deactivate")]
        public async Task<IActionResult> DeactivateMyAccount(
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerIdentity(
                out var userId,
                out _))
            {
                return Forbid();
            }

            var result = await sender.Send(
                new DeactivateMyAccountCommand(userId),
                cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Customer account not found or already inactive."
                });
            }

            return Ok(new
            {
                message = "Your customer account has been deactivated successfully."
            });
        }

        // ADMIN ONLY: Get all active customers
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(
                    new GetAllCustomersQuery(
                        pageNumber,
                        pageSize,
                        search),
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ADMIN ONLY: Get customer by ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetCustomerByIdQuery(id),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }

            return Ok(result);
        }

        // ADMIN ONLY: Deactivate customer
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeactivateCustomer(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new DeleteCustomerCommand(id),
                cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Customer not found or already inactive."
                });
            }

            return Ok(new
            {
                message = $"Customer {id} was deactivated successfully."
            });
        }

        // ADMIN ONLY: Reactivate customer
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/reactivate")]
        public async Task<IActionResult> ReactivateCustomer(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new ReactivateCustomerCommand(id),
                cancellationToken);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Customer not found or already active."
                });
            }

            return Ok(new
            {
                message = $"Customer {id} was reactivated successfully."
            });
        }

        private bool TryGetCustomerIdentity(
            out int userId,
            out int customerId)
        {
            userId = 0;
            customerId = 0;

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var customerIdValue = User.FindFirstValue("CustomerId");

            return int.TryParse(userIdValue, out userId) &&
                   int.TryParse(customerIdValue, out customerId);
        }

        // ADMIN ONLY: Get inactive customers
        [Authorize(Roles = "Admin")]
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactiveCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await sender.Send(
                    new GetInactiveCustomersQuery(
                        pageNumber,
                        pageSize,
                        search),
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

