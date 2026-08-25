// <copyright file="GlobalExceptionHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  private readonly IProblemDetailsService problemDetailsService = problemDetailsService;
  private readonly ILogger<GlobalExceptionHandler> logger = logger;

  /// <inheritdoc/>
  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    this.logger.LogError(exception, "Unhandled Exception Occurred");

    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

    return await this.problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails
      {
        Type = exception.GetType().Name,
        Title = "An Error Occurred",
        Detail = exception.Message,
      },
    });
  }
}
