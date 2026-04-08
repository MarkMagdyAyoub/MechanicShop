using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MechanicShop.Api.OpenApi.Transformers;

public sealed class BearerSecuritySchemePerOperationTransformer : IOpenApiOperationTransformer
{
  private const string SchemeId = JwtBearerDefaults.AuthenticationScheme;
  
  public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
  {
    if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
    {
      operation.Security ??= [];
      
      var key = new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference()
        {
          Type = ReferenceType.SecurityScheme,
          Id = SchemeId
        }
      };

      var requirement = new OpenApiSecurityRequirement
      {
        {key , []}
      };


      operation.Security.Add(requirement);
    }
    return Task.CompletedTask;
  }
}