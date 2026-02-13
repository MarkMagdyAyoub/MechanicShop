namespace MechanicShop.Domain.Common.Results;
public readonly record struct Error
{
  private Error(string code , string description , ErrorKind error)
  {
    Code = code;
    Description = description;
    Type = error;
  }

  public static Error Failure(string code = nameof(Failure) , string description = "General Failure")
    => new(code , description , ErrorKind.Failure);

  public static Error UnExpected(string code = nameof(UnExpected) , string description = "UnExpected Error")
    => new(code , description , ErrorKind.Unexpected);

  public static Error Validation(string code = nameof(Validation) , string description = "Validation Error")
    => new(code , description , ErrorKind.Validation);

  public static Error Conflict(string code = nameof(Conflict) , string description = "Conflict Error")
    => new(code , description , ErrorKind.Conflict);

  public static Error NotFound(string code = nameof(NotFound) , string description = "NotFound Error")
    => new(code , description , ErrorKind.NotFound);

  public static Error Unauthorized(string code = nameof(Unauthorized) , string description = "Unauthorized Error")
    => new(code , description , ErrorKind.Unauthorized);

  public static Error Forbidden(string code = nameof(Forbidden) , string description = "Forbidden Error")
    => new(code , description , ErrorKind.Forbidden);

  public static Error Create(int type , string code , string description)
    => new(code , description , (ErrorKind)type);

  public string Code { get; }
  public string Description { get; }
  public ErrorKind Type { get; }
}
