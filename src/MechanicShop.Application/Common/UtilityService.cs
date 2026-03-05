namespace MechanicShop.Application.Common;

public static class UtilityService
{
  public static string MaskEmail(string email)
  {
    int atIndex = email.IndexOf('@');
    if(atIndex <= 1)
    {
      return $"****{email.Substring(atIndex)}";
    }

    return $"{email[0]}******{email[atIndex - 1]}{email[atIndex..]}";
  }
}