using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MechanicShop.Api.OpenApi.Transformers;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
  private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;
  public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
    {
      Type = SecuritySchemeType.Http,
      Scheme = SchemeId,
      BearerFormat = "Json Web Token",
      Description = "Enter JWT Bearer Token",
      In = ParameterLocation.Header,
      Name = "Authorization",
      Reference = new OpenApiReference
      {
        Type = ReferenceType.SecurityScheme,
        Id = SchemeId
      }
    }; 

    return Task.CompletedTask;
  }
}