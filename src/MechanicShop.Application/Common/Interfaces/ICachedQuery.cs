using MediatR;

namespace MechanicShop.Application.Common.Interfaces;

public interface ICachedQuery
{
  public string CacheKey { get; }
  public string[] Tags { get; }
  public TimeSpan Expiration { get; }
}

public interface ICachedQuery<TRequest> : IRequest<TRequest> , ICachedQuery;