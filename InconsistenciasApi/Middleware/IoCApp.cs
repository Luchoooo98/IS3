using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace InconsistenciasApi.Middleware
{
    public static class IoCApp
    {
        public static IApplicationBuilder AddApplicationBuilder(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            AddUseCors(applicationBuilder);
            AddUseSwagger(applicationBuilder, configuration);

            return applicationBuilder;
        }

        private static void AddUseCors(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseCors("CORS");
        }

        private static void AddUseSwagger(IApplicationBuilder applicationBuilder, IConfiguration configuration)
        {
            applicationBuilder.UseSwagger();
            applicationBuilder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"../swagger/v{configuration["Project_Version"]}/swagger.json", "InconsistenciasAPI");
            });
        }
    }
}
