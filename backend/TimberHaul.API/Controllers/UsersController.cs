using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly TimberHaulDbContext _context;

    public UsersController(TimberHaulDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Forester")]
    [HttpGet("drivers")]
    public async Task<IActionResult> GetDrivers()
    {
        var drivers = await _context.Users
            .Where(u => u.Role == UserRole.Delivery)
            .Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Drivers retrieved successfully",
            Data = drivers
        });
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("foresters")]
    public async Task<IActionResult> GetForesters()
    {
        var foresters = await _context.Users
            .Where(u => u.Role == UserRole.Forester)
            .Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Phone
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Foresters retrieved successfully",
            Data = foresters
        });
    }
}