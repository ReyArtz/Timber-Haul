using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimberHaul.API.DTOs;
using TimberHaul.API.Services;

namespace TimberHaul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoadsController : ControllerBase
{
    private readonly ILoadService _loadService;

    public LoadsController(ILoadService loadService)
    {
        _loadService = loadService;
    }

    [Authorize(Roles = "Forester")]
    [HttpPost]
    public async Task<IActionResult> CreateLoad([FromBody] CreateLoadDto dto)
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
        var result = await _loadService.CreateLoadAsync(foresterId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Delivery")]
    [HttpGet("my-loads")]
    public async Task<IActionResult> GetMyLoads()
    {
        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _loadService.GetDriverLoadsAsync(driverId);
        return Ok(result);
    }

    [Authorize(Roles = "Forester")]
    [HttpGet("forester-loads")]
    public async Task<IActionResult> GetForesterLoads()
    {
        var foresterId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _loadService.GetForesterLoadsAsync(foresterId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLoadById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _loadService.GetLoadByIdAsync(id, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Delivery")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateLoadStatus(Guid id, [FromBody] UpdateLoadStatusDto dto)
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

        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _loadService.UpdateLoadStatusAsync(id, driverId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [Authorize(Roles = "Delivery")]
    [HttpPost("{id}/photos")]
    public async Task<IActionResult> UploadPhoto(Guid id, [FromBody] UploadPhotoDto dto)
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

        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        var result = await _loadService.UploadPhotoAsync(id, driverId, dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}