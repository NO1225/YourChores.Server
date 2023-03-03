using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourChores.Data.DataAccess;

namespace YourChores.Relational.MySQL.Setup
{
    /// <summary>
    /// Authentication wrappper 
    /// </summary>
    public static class DBContextExtensions
    {
        /// <summary>
        /// Configure the toekn authentication
        /// </summary>
        /// <param name="services">Service colleciton</param>
        /// <param name="config">Congigeration of the API</param>
        /// <returns></returns>
        public static IServiceCollection AddMySQLDBContext(this IServiceCollection services, IConfiguration config)
        {
            // Add the database with the deault connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(GetConnectionStringMySQL(config), b => b.MigrationsAssembly("YourChores.Relational.MySQL"));

            });

            return services;
        }

        /// <summary>
        /// Getting the connection string from the enveromental variables, or app settings
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string GetConnectionStringMySQL(IConfiguration config)
        {
            string hostServer = config["MYSQL_SERVICE_HOST"] ?? "localhost";
            string serverPort = config["MYSQL_SERVICE_PORT"] ?? "3306";
            string databaseName = config["MYSQL_DATABASE"] ?? "YourChoresDb1";
            string userName = config["MYSQL_USER"] ?? "root";
            string password = config["MYSQL_PASSWORD"] ?? "";

            string connectionString;

            if (string.IsNullOrEmpty(userName))
            {
                connectionString = config.GetConnectionString("Default");
            }
            else
            {
                connectionString = $"Server={hostServer};Port={serverPort};Database={databaseName};Uid={userName};Pwd={password};";
            }

            return connectionString;
        }
    }
}
