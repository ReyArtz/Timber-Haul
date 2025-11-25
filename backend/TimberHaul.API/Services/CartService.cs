using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface ICartService
{
    Task<ApiResponse<CartSummaryDto>> GetCartAsync(Guid customerId);
    Task<ApiResponse<CartItemResponseDto>> AddToCartAsync(Guid customerId, AddToCartDto dto);
    Task<ApiResponse<CartItemResponseDto>> UpdateCartItemAsync(Guid cartItemId, Guid customerId, UpdateCartItemDto dto);
    Task<ApiResponse<bool>> RemoveFromCartAsync(Guid cartItemId, Guid customerId);
    Task<ApiResponse<bool>> ClearCartAsync(Guid customerId);
}

public class CartService : ICartService
{
    private readonly TimberHaulDbContext _context;

    public CartService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CartSummaryDto>> GetCartAsync(Guid customerId)
    {
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.CustomerId == customerId)
            .Select(c => new CartItemResponseDto
            {
                CartItemId = c.CartItemId,
                CustomerId = c.CustomerId,
                ProductId = c.ProductId,
                ProductName = c.Product.ProductName,
                WoodType = c.Product.WoodType,
                Volume = c.Volume,
                PricePerUnit = c.PricePerUnit,
                TotalPrice = c.Volume * c.PricePerUnit,
                ProductImage = c.Product.ProductImage,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var summary = new CartSummaryDto
        {
            Items = cartItems,
            TotalItems = cartItems.Count,
            TotalVolume = cartItems.Sum(i => i.Volume),
            TotalAmount = cartItems.Sum(i => i.TotalPrice)
        };

        return new ApiResponse<CartSummaryDto>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = summary
        };
    }

    public async Task<ApiResponse<CartItemResponseDto>> AddToCartAsync(Guid customerId, AddToCartDto dto)
    {
        var customerExists = await _context.CustomerProfiles.AnyAsync(c => c.CustomerId == customerId);
        if (!customerExists)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = "Customer profile not found"
            };
        }

        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (!product.IsAvailable)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = "Product is not available"
            };
        }

        if (dto.Volume < product.MinOrderVolume)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = $"Minimum order volume is {product.MinOrderVolume} m³"
            };
        }

        if (dto.Volume > product.AvailableStock)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = $"Only {product.AvailableStock} m³ available in stock"
            };
        }

        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == dto.ProductId);

        if (existingCartItem != null)
        {
            existingCartItem.Volume = dto.Volume;
            existingCartItem.PricePerUnit = product.PricePerUnit;
            existingCartItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existingCartItem = new CartItem
            {
                CustomerId = customerId,
                ProductId = dto.ProductId,
                Volume = dto.Volume,
                PricePerUnit = product.PricePerUnit
            };
            _context.CartItems.Add(existingCartItem);
        }

        await _context.SaveChangesAsync();

        var cartItemDto = new CartItemResponseDto
        {
            CartItemId = existingCartItem.CartItemId,
            CustomerId = existingCartItem.CustomerId,
            ProductId = existingCartItem.ProductId,
            ProductName = product.ProductName,
            WoodType = product.WoodType,
            Volume = existingCartItem.Volume,
            PricePerUnit = existingCartItem.PricePerUnit,
            TotalPrice = existingCartItem.Volume * existingCartItem.PricePerUnit,
            ProductImage = product.ProductImage,
            CreatedAt = existingCartItem.CreatedAt
        };

        return new ApiResponse<CartItemResponseDto>
        {
            Success = true,
            Message = "Product added to cart successfully",
            Data = cartItemDto
        };
    }

    public async Task<ApiResponse<CartItemResponseDto>> UpdateCartItemAsync(Guid cartItemId, Guid customerId, UpdateCartItemDto dto)
    {
        var cartItem = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.CartItemId == cartItemId);

        if (cartItem == null)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = "Cart item not found"
            };
        }

        if (cartItem.CustomerId != customerId)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = "You can only update your own cart items"
            };
        }

        if (dto.Volume < cartItem.Product.MinOrderVolume)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = $"Minimum order volume is {cartItem.Product.MinOrderVolume} m³"
            };
        }

        if (dto.Volume > cartItem.Product.AvailableStock)
        {
            return new ApiResponse<CartItemResponseDto>
            {
                Success = false,
                Message = $"Only {cartItem.Product.AvailableStock} m³ available in stock"
            };
        }

        cartItem.Volume = dto.Volume;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var cartItemDto = new CartItemResponseDto
        {
            CartItemId = cartItem.CartItemId,
            CustomerId = cartItem.CustomerId,
            ProductId = cartItem.ProductId,
            ProductName = cartItem.Product.ProductName,
            WoodType = cartItem.Product.WoodType,
            Volume = cartItem.Volume,
            PricePerUnit = cartItem.PricePerUnit,
            TotalPrice = cartItem.Volume * cartItem.PricePerUnit,
            ProductImage = cartItem.Product.ProductImage,
            CreatedAt = cartItem.CreatedAt
        };

        return new ApiResponse<CartItemResponseDto>
        {
            Success = true,
            Message = "Cart item updated successfully",
            Data = cartItemDto
        };
    }

    public async Task<ApiResponse<bool>> RemoveFromCartAsync(Guid cartItemId, Guid customerId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.CartItemId == cartItemId);

        if (cartItem == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Cart item not found"
            };
        }

        if (cartItem.CustomerId != customerId)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "You can only remove your own cart items"
            };
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Item removed from cart successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(Guid customerId)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Cart cleared successfully",
            Data = true
        };
    }
}