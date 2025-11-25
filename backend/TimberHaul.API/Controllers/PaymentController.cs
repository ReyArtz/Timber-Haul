using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimberHaul.API.DTOs;
using TimberHaul.API.Services;

namespace TimberHaul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize(Roles = "Forester")]
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid input",
                Data = ModelState
            });
        }

        var foresterId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _paymentService.CreatePaymentAsync(foresterId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{id}/record")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid input",
                Data = ModelState
            });
        }

        var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _paymentService.RecordPaymentAsync(id, customerId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my-payments")]
    public async Task<IActionResult> GetMyPayments()
    {
        var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _paymentService.GetCustomerPaymentsAsync(customerId);
        return Ok(result);
    }

    [Authorize(Roles = "Forester")]
    [HttpGet("forester-payments")]
    public async Task<IActionResult> GetForesterPayments()
    {
        var foresterId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _paymentService.GetForesterPaymentsAsync(foresterId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _paymentService.GetPaymentByIdAsync(id, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetPaymentSummary()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var result = await _paymentService.GetPaymentSummaryAsync(userId, role);
        return Ok(result);
    }
}