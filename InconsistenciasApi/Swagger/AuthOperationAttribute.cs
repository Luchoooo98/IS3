using InconsistenciasApi.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace InconsistenciasApi.Swagger
{
    public class AuthOperationAttribute : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isAuthorized = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();
            var allowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
            var isApiKey = context.MethodInfo.GetCustomAttributes(true).OfType<ApiKeyAuthAttribute>().Any();

            if (isApiKey)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                var apiKeyScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ apiKeyScheme ] = new List<string>()
                    }
                };
            }
            else
            {
                if (isAuthorized && !allowAnonymous)
                {
                    operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                    operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                    var bearerScheme = new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    };

                    operation.Security = new List<OpenApiSecurityRequirement>
                    {
                        new OpenApiSecurityRequirement
                        {
                            [ bearerScheme ] = new List<string>()
                        }
                    };
                }
            }
        }
    }
}
