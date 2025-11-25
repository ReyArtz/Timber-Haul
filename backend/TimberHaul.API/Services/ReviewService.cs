using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface IReviewService
{
    Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(Guid customerId, CreateReviewDto dto);
    Task<ApiResponse<List<ReviewListDto>>> GetLoadReviewsAsync(Guid loadId);
    Task<ApiResponse<List<ReviewListDto>>> GetDriverReviewsAsync(Guid driverId);
}

public class ReviewService : IReviewService
{
    private readonly TimberHaulDbContext _context;

    public ReviewService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<ReviewResponseDto>> CreateReviewAsync(Guid customerId, CreateReviewDto dto)
    {
        var load = await _context.Loads
            .Include(l => l.Driver)
                .ThenInclude(d => d!.User)
            .FirstOrDefaultAsync(l => l.LoadId == dto.LoadId);

        if (load == null)
        {
            return new ApiResponse<ReviewResponseDto>
            {
                Success = false,
                Message = "Load not found"
            };
        }

        if (load.CustomerId != customerId)
        {
            return new ApiResponse<ReviewResponseDto>
            {
                Success = false,
                Message = "You can only review your own deliveries"
            };
        }

        if (load.Status != LoadStatus.Delivered)
        {
            return new ApiResponse<ReviewResponseDto>
            {
                Success = false,
                Message = "You can only review completed deliveries"
            };
        }

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.LoadId == dto.LoadId && r.CustomerId == customerId);

        if (existingReview != null)
        {
            return new ApiResponse<ReviewResponseDto>
            {
                Success = false,
                Message = "You have already reviewed this delivery"
            };
        }

        var review = new Review
        {
            LoadId = dto.LoadId,
            CustomerId = customerId,
            DriverId = load.DriverId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        _context.Reviews.Add(review);

        if (load.DriverId.HasValue)
        {
            var driver = await _context.DeliveryProfiles
                .FirstOrDefaultAsync(d => d.DriverId == load.DriverId.Value);

            if (driver != null)
            {
                var allReviews = await _context.Reviews
                    .Where(r => r.DriverId == load.DriverId.Value)
                    .ToListAsync();

                allReviews.Add(review);
                driver.Rating = (decimal)allReviews.Average(r => r.Rating);
            }
        }

        await _context.SaveChangesAsync();

        var customer = await _context.CustomerProfiles
            .Include(c => c.User)
            .FirstAsync(c => c.CustomerId == customerId);

        var reviewDto = new ReviewResponseDto
        {
            ReviewId = review.ReviewId,
            LoadId = review.LoadId,
            LoadNumber = load.LoadNumber,
            CustomerId = customerId,
            CustomerName = customer.User.FirstName + " " + customer.User.LastName,
            DriverId = review.DriverId,
            DriverName = load.Driver != null ? load.Driver.User.FirstName + " " + load.Driver.User.LastName : null,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return new ApiResponse<ReviewResponseDto>
        {
            Success = true,
            Message = "Review created successfully",
            Data = reviewDto
        };
    }

    public async Task<ApiResponse<List<ReviewListDto>>> GetLoadReviewsAsync(Guid loadId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.LoadId == loadId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewListDto
            {
                ReviewId = r.ReviewId,
                CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<ReviewListDto>>
        {
            Success = true,
            Message = "Reviews retrieved successfully",
            Data = reviews
        };
    }

    public async Task<ApiResponse<List<ReviewListDto>>> GetDriverReviewsAsync(Guid driverId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.DriverId == driverId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewListDto
            {
                ReviewId = r.ReviewId,
                CustomerName = r.Customer.User.FirstName + " " + r.Customer.User.LastName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<ReviewListDto>>
        {
            Success = true,
            Message = "Reviews retrieved successfully",
            Data = reviews
        };
    }
}