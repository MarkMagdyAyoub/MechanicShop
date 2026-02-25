namespace MechanicShop.Application.Features.Identity.DTOs;

public class TokenDto
{
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
  public DateTime Expiration { get; set; }
}