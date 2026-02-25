using System.Security.Claims;
using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
  IAppDbContext context,
  ILogger<RefreshTokenCommandHandler> logger,
  ITokenProvider tokenProvider,
  IIdentityService identityService
) : IRequestHandler<RefreshTokenCommand, Result<TokenDto>>
{
  private readonly IAppDbContext _context = context;
  private readonly ILogger<RefreshTokenCommandHandler> _logger = logger;
  private readonly ITokenProvider _tokenProvider = tokenProvider;
  private readonly IIdentityService _identityService = identityService;

  public async Task<Result<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
  {
    var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

    if(principal is null)
    {
      _logger.LogWarning("Expired Access Token Is Invalid.");
      
      return ApplicationErrors.ExpiredAccessTokenInvalid;
    }

    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if(userId is null)
    {
      _logger.LogWarning("Invalid UserId Claim");
      
      return ApplicationErrors.UserIdClaimInvalid;
    }

    var getUserResult = await _identityService.GetUserByIdAsync(Guid.Parse(userId));

    if (getUserResult.IsError)
    {
      _logger.LogWarning("Get User By Id Error Occurred: {ErrorDescription}" , getUserResult.TopError.Description);
      
      return getUserResult.Errors;
    }

    var refreshToken = await _context.RefreshTokens
                              .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId , cancellationToken);

    if(refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
    {
      _logger.LogWarning("Refresh Token Has Expired");
      
      return ApplicationErrors.RefreshTokenExpired;
    }

    var generateNewAccessToken = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value , cancellationToken);

    if (generateNewAccessToken.IsError)
    {
        _logger.LogError("Generate Token Error Occurred: {ErrorDescription}", generateNewAccessToken.TopError.Description);

        return generateNewAccessToken.Errors;
    }

    return generateNewAccessToken.Value;
  }
}