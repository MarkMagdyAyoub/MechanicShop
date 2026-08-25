// <copyright file="ApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Controllers;

using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

[ApiController]
public class ApiController : ControllerBase
{
  protected ActionResult ProblemDetailsHandler(List<Error> errors)
  {
    if (errors.Count == 0)
    {
      return this.Problem();
    }

    if (errors.All(error => error.Type == ErrorKind.Validation))
    {
      return this.ValidationProblem(errors);
    }

    return this.Problem(errors[0]);
  }

  private ObjectResult Problem(Error error)
  {
    var statusCode = error.Type switch
    {
      ErrorKind.Conflict => StatusCodes.Status409Conflict,
      ErrorKind.Validation => StatusCodes.Status400BadRequest,
      ErrorKind.NotFound => StatusCodes.Status404NotFound,
      ErrorKind.Unauthorized => StatusCodes.Status404NotFound,
      _ => StatusCodes.Status500InternalServerError,
    };

    return this.Problem(statusCode: statusCode, title: error.Description);
  }

  private ActionResult ValidationProblem(List<Error> errors)
  {
    var modelState = new ModelStateDictionary();
    errors.ForEach(error => modelState.AddModelError(error.Code, error.Description));

    return this.ValidationProblem(modelState);
  }
}
