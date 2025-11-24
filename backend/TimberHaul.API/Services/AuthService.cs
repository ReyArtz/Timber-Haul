using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
}

public class AuthService : IAuthService
{
    private readonly TimberHaulDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(TimberHaulDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Email already exists"
            };
        }

        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Phone = registerDto.Phone,
            Role = registerDto.Role,
            IsActive = true
        };

        _context.Users.Add(user);

        switch (registerDto.Role)
        {
            case UserRole.Forester:
                _context.ForesterProfiles.Add(new ForesterProfile
                {
                    ForesterId = user.UserId
                });
                break;

            case UserRole.Delivery:
                _context.DeliveryProfiles.Add(new DeliveryProfile
                {
                    DriverId = user.UserId
                });
                break;

            case UserRole.Customer:
                _context.CustomerProfiles.Add(new CustomerProfile
                {
                    CustomerId = user.UserId
                });
                break;
        }

        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Registration successful",
            Data = new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                UserId = user.UserId
            }
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        if (!user.IsActive)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Account is inactive"
            };
        }

        var token = GenerateJwtToken(user);

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                UserId = user.UserId
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            }),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}