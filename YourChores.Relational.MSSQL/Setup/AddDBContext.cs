using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourChores.Data.DataAccess;

namespace YourChores.Relational.MSSQL.Setup
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
        public static IServiceCollection AddMSSQLDBContext(this IServiceCollection services, IConfiguration config)
        {
            // Add the database with the deault connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(GetConnectionStringSqlServer(config), b => b.MigrationsAssembly("YourChores.Relational.MSSQL"));
            });

            return services;
        }

        /// <summary>
        /// Getting the connection string from the enveromental variables, or app settings
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string GetConnectionStringSqlServer(IConfiguration config)
        {
            string hostServer = config["HOST_SERVER"] ?? "(localdb)\\MSSQLLocalDB";
            string serverPort = config["HOST_PORT"] ?? "1433";
            string databaseName = config["DATABASE_NAME"] ?? "YourChoresDb";
            string userName = config["USERNAME"];
            string passward = config["SA_PASSWORD"];

            string connectionString;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passward))
            {
                connectionString = config.GetConnectionString("Default");
            }
            else
            {
                connectionString = $"Server={hostServer},{serverPort};Database={databaseName};User Id={userName};Password={passward};";
            }

            return connectionString;
        }
    }
}
