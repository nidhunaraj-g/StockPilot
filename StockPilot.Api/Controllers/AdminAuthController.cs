using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Application.Features.Admin.Commands;
using StockPilot.Application.Features.Admin.DTOs;

namespace StockPilot.Api.Controllers
{
    [Route("api/admin-auth")]
    [ApiController]
    public class AdminAuthController(ISender sender) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            AdminLoginRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await sender.Send(
                    new AdminLoginCommand(request),
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
    }
}