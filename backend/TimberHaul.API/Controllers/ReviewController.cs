using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimberHaul.API.DTOs;
using TimberHaul.API.Services;

namespace TimberHaul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
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
        var result = await _reviewService.CreateReviewAsync(customerId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("load/{loadId}")]
    public async Task<IActionResult> GetLoadReviews(Guid loadId)
    {
        var result = await _reviewService.GetLoadReviewsAsync(loadId);
        return Ok(result);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<IActionResult> GetDriverReviews(Guid driverId)
    {
        var result = await _reviewService.GetDriverReviewsAsync(driverId);
        return Ok(result);
    }
}