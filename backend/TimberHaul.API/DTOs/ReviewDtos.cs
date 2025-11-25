using System.ComponentModel.DataAnnotations;

namespace TimberHaul.API.DTOs;

public class CreateReviewDto
{
    [Required(ErrorMessage = "Load ID is required")]
    public Guid LoadId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string? Comment { get; set; }
}

public class ReviewResponseDto
{
    public Guid ReviewId { get; set; }
    public Guid LoadId { get; set; }
    public string LoadNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewListDto
{
    public Guid ReviewId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}