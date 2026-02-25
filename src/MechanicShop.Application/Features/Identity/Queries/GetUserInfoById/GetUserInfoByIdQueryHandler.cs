using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;

public sealed class GetUserInfoByIdQueryHandler(
  IIdentityService identityService,
  ILogger<GetUserInfoByIdQueryHandler> logger
) : IRequestHandler<GetUserInfoByIdQuery, Result<UserDto>>
{
  private readonly IIdentityService _identityService = identityService;
  private readonly ILogger<GetUserInfoByIdQueryHandler> _logger = logger;

  public async Task<Result<UserDto>> Handle(GetUserInfoByIdQuery request, CancellationToken cancellationToken)
  {
    var getUserByIdResult = await _identityService.GetUserByIdAsync(request.UserId);

    if (getUserByIdResult.IsError)
    {
      _logger.LogWarning("User With Id `{UserId}`: {ErrorDescription}" , request.UserId , getUserByIdResult.TopError.Description);

      return getUserByIdResult.Errors;
    }

    return getUserByIdResult.Value;
  }
}