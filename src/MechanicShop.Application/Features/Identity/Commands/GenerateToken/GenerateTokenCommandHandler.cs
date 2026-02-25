using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Commands.GenerateToken;

public sealed class GenerateTokenCommandHandler(
  ILogger<GenerateTokenCommandHandler> logger,
  IIdentityService identityService,
  ITokenProvider tokenProvider
) : IRequestHandler<GenerateTokenCommand, Result<TokenDto>>
{
  private readonly ILogger<GenerateTokenCommandHandler> _logger = logger;
  private readonly IIdentityService _identityService = identityService;
  private readonly ITokenProvider _tokenProvider = tokenProvider;

  public async Task<Result<TokenDto>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
  {
    var userResponse = await _identityService.AuthenticateUserAsync(request.Email , request.Password , cancellationToken);

    if (userResponse.IsError)
    {
      return userResponse.Errors;
    }

    var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(userResponse.Value , cancellationToken);

    if (generateTokenResult.IsError)
    {
      _logger.LogWarning("Generate Token Error Occurred: {ErrorDescription}" , generateTokenResult.TopError.Description);
      return generateTokenResult.Errors;
    }

    return generateTokenResult.Value;
  }
}