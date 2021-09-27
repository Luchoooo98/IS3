using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InconsistenciasApi.Swagger;
using Microsoft.EntityFrameworkCore;
using InconsistenciasApi.Services;
using InconsistenciasApi.Services.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using InconsistenciasApi.Models.Entities;

namespace InconsistenciasApi.Middleware
{
    public static class IoC
    {
        public static IServiceCollection AddRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddLogging();
            AddCors(services);
            AddSwaggerService(services, configuration);
            AddJwtService(services, configuration);
            AddServicesClass(services);
            AddConfigDatabase(services, configuration);

            return services;
        }

        private static void AddConfigDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MyContextDatabase>(options => options.UseSqlServer(configuration["ConnectionString"], sqlServer => sqlServer.MigrationsAssembly("InconsistenciasApi")), ServiceLifetime.Scoped);
        }

        private static void AddServicesClass(IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IArchivoService, ArchivoService>();
        }

        private static void AddCors(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("CORS", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // CONFIGURACIÓN DEL SERVICIO DE AUTENTICACIÓN JWT
        private static void AddJwtService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                           .AddJwtBearer(options =>
                           {
                               //options.RequireHttpsMetadata = false;
                               options.SaveToken = true;
                               options.TokenValidationParameters = new TokenValidationParameters()
                               {
                                   ClockSkew = TimeSpan.Zero,
                                   ValidateIssuer = true,
                                   ValidateAudience = true,
                                   ValidateLifetime = true,
                                   ValidateIssuerSigningKey = true,
                                   ValidIssuer = configuration["JWT:Issuer"],
                                   ValidAudience = configuration["JWT:Audience"],
                                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]))
                               };
                           });
        }

        //Swagger API Doc
        private static void AddSwaggerService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"v{configuration["Project_Version"]}", new OpenApiInfo
                {
                    Version = $"v{configuration["Project_Version"]}",
                    Title = "Test de Inconsistencias API",
                    Description = "Microservicio de Test de Inconsistencias"
                });

                #region Clase para filtrar que apis requieren determinado esquema de seguridad - Activado
                c.OperationFilter<AuthOperationAttribute>();
                #endregion
                EspecificarEsquemasDeSeguridad(c);

                #region GLOBAL REQUERIMIENTO DE SEGURIDAD - Desactivado
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                //{
                //    {
                //      new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "ApiKey"
                //            },
                //        },
                //        new List<string>()
                //    }
                //});

                //c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                //{
                //    {
                //      new OpenApiSecurityScheme
                //        {
                //            Reference = new OpenApiReference
                //            {
                //                Type = ReferenceType.SecurityScheme,
                //                Id = "Bearer"
                //            },
                //        },
                //        new List<string>()
                //    }
                //});
                #endregion

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        private static void EspecificarEsquemasDeSeguridad(SwaggerGenOptions c)
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Use bearer token to authorize",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            //c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            //{
            //    Description = "Enter your Api Key below:",
            //    Name = "ApiKey",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.ApiKey
            //});
        }
    }
}
