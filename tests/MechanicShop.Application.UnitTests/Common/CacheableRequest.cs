using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Application.UnitTests.Common;

public class CacheableRequest : ICachedQuery
{
  public string CacheKey => "test";

  public string CacheValue = "data";

  public string[] Tags => ["tests"];

  public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}