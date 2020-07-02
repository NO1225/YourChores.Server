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
using YourChores.Relational.MySQL.Setup;
using YourChores.Server.Authentication;
using YourChores.Server.OpenAPI;

namespace YourChores.Server
{
    /// <summary>
    /// The startup of our server
    /// </summary>
    public class Startup
    {

        private readonly IConfiguration _configuration;

        private readonly string corsPolicy = "AllowOrigins";

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
            //services.AddMSSQLDBContext(_configuration);

            // Add the database with the deault connection string
            services.AddMySQLDBContext(_configuration);

            // Adding the idintity (login/register)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                // Store it in this database
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // Custome error dexriper with chosen language
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
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

            // Adding swagger implementation
            services.AddSwagger();

            // Adding Cors
            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicy,
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                    });
            });

            services.AddControllers();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="serviceProvider"></param>
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
                app.ConfigureSwagger();
            }

            // Adding Cors
            app.UseCors(corsPolicy);

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

        #endregion
    }
}
