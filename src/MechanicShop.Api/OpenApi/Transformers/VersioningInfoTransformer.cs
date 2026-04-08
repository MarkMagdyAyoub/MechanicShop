using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MechanicShop.Api.OpenApi.Transformers;

public sealed class VersioningInfoTransformer : IOpenApiDocumentTransformer
{
  private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;
  
  public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    var version = context.DocumentName;

    document.Info.Version = version;
    document.Info.Title = $"MechanicShop {version}";

    return Task.CompletedTask;
  }
}