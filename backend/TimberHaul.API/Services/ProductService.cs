using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface IProductService
{
    Task<ApiResponse<List<ProductResponseDto>>> GetAllProductsAsync();
    Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(Guid productId);
    Task<ApiResponse<ProductResponseDto>> CreateProductAsync(Guid foresterId, CreateProductDto dto);
    Task<ApiResponse<ProductResponseDto>> UpdateProductAsync(Guid productId, Guid foresterId, UpdateProductDto dto);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid productId, Guid foresterId);
    Task<ApiResponse<List<ProductResponseDto>>> GetProductsByForesterAsync(Guid foresterId);
}

public class ProductService : IProductService
{
    private readonly TimberHaulDbContext _context;

    public ProductService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<ProductResponseDto>>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Forester)
                .ThenInclude(f => f.User)
            .Where(p => p.IsAvailable)
            .Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                ForesterId = p.ForesterId,
                ForesterName = p.Forester.User.FirstName + " " + p.Forester.User.LastName,
                ProductName = p.ProductName,
                WoodType = p.WoodType,
                Description = p.Description,
                PricePerUnit = p.PricePerUnit,
                MinOrderVolume = p.MinOrderVolume,
                AvailableStock = p.AvailableStock,
                ProductImage = p.ProductImage,
                IsAvailable = p.IsAvailable,
                CreatedAt = p.CreatedAt
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return new ApiResponse<List<ProductResponseDto>>
        {
            Success = true,
            Message = "Products retrieved successfully",
            Data = products
        };
    }

    public async Task<ApiResponse<ProductResponseDto>> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Forester)
                .ThenInclude(f => f.User)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            return new ApiResponse<ProductResponseDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        var productDto = new ProductResponseDto
        {
            ProductId = product.ProductId,
            ForesterId = product.ForesterId,
            ForesterName = product.Forester.User.FirstName + " " + product.Forester.User.LastName,
            ProductName = product.ProductName,
            WoodType = product.WoodType,
            Description = product.Description,
            PricePerUnit = product.PricePerUnit,
            MinOrderVolume = product.MinOrderVolume,
            AvailableStock = product.AvailableStock,
            ProductImage = product.ProductImage,
            IsAvailable = product.IsAvailable,
            CreatedAt = product.CreatedAt
        };

        return new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = productDto
        };
    }

    public async Task<ApiResponse<ProductResponseDto>> CreateProductAsync(Guid foresterId, CreateProductDto dto)
    {
        var foresterExists = await _context.ForesterProfiles.AnyAsync(f => f.ForesterId == foresterId);
        if (!foresterExists)
        {
            return new ApiResponse<ProductResponseDto>
            {
                Success = false,
                Message = "Forester profile not found"
            };
        }

        var product = new Product
        {
            ForesterId = foresterId,
            ProductName = dto.ProductName,
            WoodType = dto.WoodType,
            Description = dto.Description,
            PricePerUnit = dto.PricePerUnit,
            MinOrderVolume = dto.MinOrderVolume,
            AvailableStock = dto.AvailableStock,
            ProductImage = dto.ProductImage,
            IsAvailable = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var forester = await _context.ForesterProfiles
            .Include(f => f.User)
            .FirstAsync(f => f.ForesterId == foresterId);

        var productDto = new ProductResponseDto
        {
            ProductId = product.ProductId,
            ForesterId = product.ForesterId,
            ForesterName = forester.User.FirstName + " " + forester.User.LastName,
            ProductName = product.ProductName,
            WoodType = product.WoodType,
            Description = product.Description,
            PricePerUnit = product.PricePerUnit,
            MinOrderVolume = product.MinOrderVolume,
            AvailableStock = product.AvailableStock,
            ProductImage = product.ProductImage,
            IsAvailable = product.IsAvailable,
            CreatedAt = product.CreatedAt
        };

        return new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product created successfully",
            Data = productDto
        };
    }

    public async Task<ApiResponse<ProductResponseDto>> UpdateProductAsync(Guid productId, Guid foresterId, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Forester)
                .ThenInclude(f => f.User)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            return new ApiResponse<ProductResponseDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (product.ForesterId != foresterId)
        {
            return new ApiResponse<ProductResponseDto>
            {
                Success = false,
                Message = "You can only update your own products"
            };
        }

        if (dto.ProductName != null) product.ProductName = dto.ProductName;
        if (dto.WoodType != null) product.WoodType = dto.WoodType.Value;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.PricePerUnit != null) product.PricePerUnit = dto.PricePerUnit.Value;
        if (dto.MinOrderVolume != null) product.MinOrderVolume = dto.MinOrderVolume.Value;
        if (dto.AvailableStock != null) product.AvailableStock = dto.AvailableStock.Value;
        if (dto.ProductImage != null) product.ProductImage = dto.ProductImage;
        if (dto.IsAvailable != null) product.IsAvailable = dto.IsAvailable.Value;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var productDto = new ProductResponseDto
        {
            ProductId = product.ProductId,
            ForesterId = product.ForesterId,
            ForesterName = product.Forester.User.FirstName + " " + product.Forester.User.LastName,
            ProductName = product.ProductName,
            WoodType = product.WoodType,
            Description = product.Description,
            PricePerUnit = product.PricePerUnit,
            MinOrderVolume = product.MinOrderVolume,
            AvailableStock = product.AvailableStock,
            ProductImage = product.ProductImage,
            IsAvailable = product.IsAvailable,
            CreatedAt = product.CreatedAt
        };

        return new ApiResponse<ProductResponseDto>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = productDto
        };
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid productId, Guid foresterId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (product.ForesterId != foresterId)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "You can only delete your own products"
            };
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Product deleted successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<List<ProductResponseDto>>> GetProductsByForesterAsync(Guid foresterId)
    {
        var products = await _context.Products
            .Include(p => p.Forester)
                .ThenInclude(f => f.User)
            .Where(p => p.ForesterId == foresterId)
            .Select(p => new ProductResponseDto
            {
                ProductId = p.ProductId,
                ForesterId = p.ForesterId,
                ForesterName = p.Forester.User.FirstName + " " + p.Forester.User.LastName,
                ProductName = p.ProductName,
                WoodType = p.WoodType,
                Description = p.Description,
                PricePerUnit = p.PricePerUnit,
                MinOrderVolume = p.MinOrderVolume,
                AvailableStock = p.AvailableStock,
                ProductImage = p.ProductImage,
                IsAvailable = p.IsAvailable,
                CreatedAt = p.CreatedAt
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return new ApiResponse<List<ProductResponseDto>>
        {
            Success = true,
            Message = "Products retrieved successfully",
            Data = products
        };
    }
}