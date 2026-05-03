using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MechanicShop.Api.Controllers;



// please review the following concepts
// controllers
// Problem Details
[ApiController]
public class ApiController : ControllerBase
{
  protected ActionResult ProblemDetailsHandler(List<Error> errors)
  {
    if(errors.Count == 0)
      return Problem();
    
    if(errors.All(error => error.Type == ErrorKind.Validation))
      return ValidationProblem(errors);
    
    return Problem(errors[0]);
  }

  private ObjectResult Problem(Error error)
  {
    var statusCode = error.Type switch
    {
      ErrorKind.Conflict     => StatusCodes.Status409Conflict,
      ErrorKind.Validation   => StatusCodes.Status400BadRequest,
      ErrorKind.NotFound     => StatusCodes.Status404NotFound,
      ErrorKind.Unauthorized => StatusCodes.Status404NotFound,
      _ => StatusCodes.Status500InternalServerError
    };

    return Problem(statusCode: statusCode , title: error.Description);
  }

  private ActionResult ValidationProblem(List<Error> errors)
  {
    var modelState = new ModelStateDictionary();
    errors.ForEach(error => modelState.AddModelError(error.Code , error.Description));

    return ValidationProblem(modelState);
  }
}
