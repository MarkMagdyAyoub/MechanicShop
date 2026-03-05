using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MechanicShop.Infrastructure.Identity;

public sealed class TokenProvider(
  IAppDbContext context , 
  IConfiguration configuration,
  ILogger<TokenProvider> logger
) : ITokenProvider
{
  private readonly IAppDbContext _context = context;
  private readonly IConfiguration _configuration = configuration;
  private readonly ILogger<TokenProvider> _logger = logger;

  public async Task<Result<TokenDto>> GenerateJwtTokenAsync(UserDto user, CancellationToken cancellationToken)
  {
    var createTokenResult = await CreateTokenAsync(user , cancellationToken);

    if (createTokenResult.IsError)
    {
      _logger.LogWarning(
        "Failed To Create New Token For User With Id `{UserId}`: {ErrorDescription} " , 
        user.userId , 
        createTokenResult.TopError.Description
      );
      
      return createTokenResult.Errors;
    }

    return createTokenResult.Value;
  }

  public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
  {
    var jwtSettings = _configuration.GetSection("JwtSettings");

    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = true,
      ValidateIssuer = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = false,
      ValidIssuer = jwtSettings["Issuer"],
      ValidAudience = jwtSettings["Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
      ClockSkew = TimeSpan.Zero
    };
    

    var tokenHandler = new JwtSecurityTokenHandler();

    var principal = tokenHandler.ValidateToken(token , tokenValidationParameters , out SecurityToken securityToken);

    if(
        securityToken is not JwtSecurityToken jwtSecurityToken || 
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256 , StringComparison.InvariantCultureIgnoreCase)
      )
    {
      throw new SecurityTokenException("Invalid Token.");
    }

    return principal;
  }

  public async Task<Result<TokenDto>> CreateTokenAsync(UserDto user, CancellationToken cancellationToken)
  {
    var jwtSettings = _configuration.GetSection("JwtSettings");
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];
    var key = jwtSettings["Key"];
    var expiration =  DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));
    var refreshTokenExpiration = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpirationInDays"]!));

    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier , user.userId.ToString()),
      new Claim(ClaimTypes.Email , user.Email)
    };

    foreach (var role in user.Roles)
      claims.Add(new Claim(ClaimTypes.Role , role));

    var descriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Issuer = issuer,
      Audience = audience,
      Expires = expiration,
      SigningCredentials = new SigningCredentials(
        key: new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
        algorithm: SecurityAlgorithms.HmacSha256Signature
      )
    };

    var tokenHandler = new JwtSecurityTokenHandler();

    var token = tokenHandler.CreateToken(descriptor);

    await _context.RefreshTokens.Where(rt => rt.UserId == user.userId).ExecuteDeleteAsync(cancellationToken);

    var newRefreshTokenResult = RefreshToken.Create(
      id: Guid.NewGuid(),
      token: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
      userId: user.userId,
      expiresOnUtc: refreshTokenExpiration
    );

    if (newRefreshTokenResult.IsError)
    {
      _logger.LogWarning("Failed To Generate A New Refresh Token To User With Id `{UserId}`: {ErrorDescription}" , user.userId , newRefreshTokenResult.TopError.Description);
      return newRefreshTokenResult.Errors;
    }

    var refreshToken = newRefreshTokenResult.Value;

    _context.RefreshTokens.Add(refreshToken);

    await _context.SaveChangesAsync(cancellationToken);

    return new TokenDto
    {
      AccessToken = tokenHandler.WriteToken(token),
      RefreshToken = newRefreshTokenResult.Value.Token,
      Expiration = expiration
    };
  }
}