// <copyright file="VersioningInfoTransformer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MechanicShop.Api.OpenApi.Transformers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

public sealed class VersioningInfoTransformer : IOpenApiDocumentTransformer
{
  private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;

  /// <inheritdoc/>
  public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    var version = context.DocumentName;

    document.Info.Version = version;
    document.Info.Title = $"MechanicShop {version}";
    document.Info.Description = $"Backend Api For Managing Streamline The Daily Operations Of A Modern Mechanic Shop.";
    document.Info.Contact = new OpenApiContact
    {
      Email = "markgeforce4080@gmail.com",
    };

    return Task.CompletedTask;
  }
}
