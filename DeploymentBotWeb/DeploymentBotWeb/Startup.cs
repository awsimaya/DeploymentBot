using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.NETCore.Setup;
using DeploymentBotWeb.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeploymentBotWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "<Your Amazon Access Key Id>");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "<Your Amazon Secret Access Key>");
            Environment.SetEnvironmentVariable("AWS_REGION", "<Your Region Name>");

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            // Register the IConfiguration instance which MyOptions binds against.
            services.Configure<AWSOptions>(Configuration.GetSection("AWSConfiguration"));
            services.AddScoped<IAWSLexService, AWSLexService>();

            //Add Session Obj 
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(300);
                options.Cookie.HttpOnly = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Login}/{id?}");
            });
        }
    }
}
