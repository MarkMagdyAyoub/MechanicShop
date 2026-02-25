using MechanicShop.Application.Features.Identity.DTOs;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfoById;

public sealed record GetUserInfoByIdQuery(
  Guid UserId
) : IRequest<Result<UserDto>>;