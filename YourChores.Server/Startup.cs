using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Threading.Tasks;
using YourChores.Data.DataAccess;
using YourChores.Data.Models;
using YourChores.Server.Authentication;

namespace YourChores.Server
{
    /// <summary>
    /// The startup of our server
    /// </summary>
    public class Startup
    {

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {

            ////Add the database with the deault connection string
            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseSqlServer(GetConnectionStringSqlServer(),b=>b.MigrationsAssembly("YourChores.Relational.MSSQL"));
            //});

            // Add the database with the deault connection string
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(GetConnectionStringMySQL(), b => b.MigrationsAssembly("YourChores.Relational.MySQL"));


            });

            // Adding the idintity (login/register)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                // Store it in this database
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // We are going to use token for authorization
                .AddDefaultTokenProviders();

            // Adding token authenrication
            services.AddTokenAuthentication(_configuration);

            // Identity options
            services.Configure<IdentityOptions>(options =>
            {
                // Very weak passward
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;

                // No dublicate emails
                options.User.RequireUniqueEmail = true;
            });

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



            services.AddControllers();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            // Use authentication
            app.UseAuthentication();

            // Use authorization
            app.UseAuthorization();

            // If is in development
            if (true || env.IsDevelopment())
            {
                // Use swagger
                app.UseSwagger();

                // Add swagger UI
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Versioned API v1.0");

                    c.DocExpansion(DocExpansion.None);
                });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            EnsureAdminCreated(serviceProvider).GetAwaiter().GetResult();
        }

        #region Helper Methods

        private async Task EnsureAdminCreated(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService<ApplicationDbContext>();
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            // ENsure database created and is up to date
            await context.Database.MigrateAsync();

            // Ensure that we have an admin role
            var adminRoleExist = await roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExist)
            {
                await roleManager.CreateAsync(new IdentityRole()
                {
                    Name = "Admin"
                });
            }

            // Ensure that we have an admin user
            var adminUser = await userManager.FindByNameAsync("appAdmin");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser()
                {
                    UserName = "appAdmin",
                    Email = "appAdmin@yourchores.com"
                };

                await userManager.CreateAsync(newAdminUser, "123123123");

                await userManager.AddToRoleAsync(newAdminUser, "admin");
            }
            else
            {
                if (!(await userManager.IsInRoleAsync(adminUser, "admin")))
                {
                    await userManager.AddToRoleAsync(adminUser, "admin");
                }
            }


        }

        private string GetConnectionStringMySQL()
        {
            string hostServer = _configuration["MYSQL_SERVICE_HOST"] ?? "localhost";
            string serverPort = _configuration["MYSQL_SERVICE_PORT"] ?? "3306";
            string databaseName = _configuration["MYSQL_DATABASE"] ?? "YourChoresDb1";
            string userName = _configuration["MYSQL_USER"] ?? "root";
            string passward = _configuration["MYSQL_PASSWORD"] ?? "";

            string connectionString;

            if (string.IsNullOrEmpty(userName))
            {
                connectionString = _configuration.GetConnectionString("Default");
            }
            else
            {
                connectionString = $"Server={hostServer};Port={serverPort};Database={databaseName};Uid={userName};Pwd={passward};";
            }

            return connectionString;
        }

        private string GetConnectionStringSqlServer()
        {
            string hostServer = _configuration["HOST_SERVER"] ?? "(localdb)\\MSSQLLocalDB";
            string serverPort = _configuration["HOST_PORT"] ?? "1433";
            string databaseName = _configuration["DATABASE_NAME"] ?? "YourChoresDb";
            string userName = _configuration["USERNAME"];
            string passward = _configuration["SA_PASSWORD"];

            string connectionString;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passward))
            {
                connectionString = _configuration.GetConnectionString("Default");
            }
            else
            {
                connectionString = $"Server={hostServer},{serverPort};Database={databaseName};User Id={userName};Password={passward};";
            }

            return connectionString;
        }
        #endregion
    }
}
