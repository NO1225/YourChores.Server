using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace YourChores.Server.Authentication
{
    /// <summary>
    /// Authentication wrappper 
    /// </summary>
    public static class AuthenticationExtension
    {
        /// <summary>
        /// Configure the toekn authentication
        /// </summary>
        /// <param name="services">Service colleciton</param>
        /// <param name="config">Congigeration of the API</param>
        /// <returns></returns>
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // Getting the secret key
            var secret = config.GetSection("Jwt").GetSection("Key").Value;

            var key = Encoding.ASCII.GetBytes(secret);

            // adding the authantication to the services
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding the jwt token authentation
            .AddJwtBearer(x =>
            {
                // Token configration
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.GetSection("Jwt").GetSection("Issuer").Value,
                    ValidAudience = config.GetSection("Jwt").GetSection("Audience").Value
                };
            });

            return services;
        }
    }
}
