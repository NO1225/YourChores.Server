using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

namespace YourChores.Server.OpenAPI
{
    /// <summary>
    /// Swagger setup
    /// </summary>
    public static class AddSwaggerImplementation
    {
        /// <summary>
        /// Configure the toekn authentication
        /// </summary>
        /// <param name="services">Service colleciton</param>
        /// <param name="config">Congigeration of the API</param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            // Adding swagger to our server
            services.AddSwaggerGen(options =>
            {
                // Adding swagger document
                options.SwaggerDoc("v1.0", new OpenApiInfo() { Title = "Main API v1.0", Version = "v1.0" });

                // To use unique names with the requests and responses
                options.CustomSchemaIds(x => x.FullName);

                // Include the comments that we wrote in the documentation
                options.IncludeXmlComments("YourChores.Server.xml");

                // Defining the security schema
                var securitySchema = new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                // Adding the bearer token authentaction option to the ui
                options.AddSecurityDefinition("Bearer", securitySchema);

                // use the token provided with the endpoints call
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                });

            });

            return services;
        }

        /// <summary>
        /// To configure the UI of the open api
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app)
        {
            // Use swagger
            app.UseSwagger();

            // Add swagger UI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Versioned API v1.0");

                c.DocExpansion(DocExpansion.None);
            });

            return app;
        }
    }
}
