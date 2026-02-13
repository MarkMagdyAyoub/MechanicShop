using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Domain.Identity;

public static class RefreshTokenErrors
{
    public static readonly Error IdRequired =
        Error.Validation("RefreshToken.Id.Required", "Refresh Token ID Is Required");

    public static readonly Error TokenRequired =
        Error.Validation("RefreshToken.Token.Required", "Token Is Required");

    public static readonly Error UserIdRequired =
        Error.Validation("RefreshToken.UserId.Required", "User Id Is Required");

    public static readonly Error PastDate =
        Error.Validation("RefreshToken.ExpiresOnUtc.Invalid", "ExpiresOnUtc Must Be In The Future");
}