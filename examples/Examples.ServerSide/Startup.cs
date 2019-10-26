using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Examples.ServerSide.Data;
using Blazor.Auth0;
using Blazor.Auth0.Models;

namespace Examples.ServerSide
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor(options=> {
                options.DetailedErrors = true;
            });

            // New Blazor.Auth0 one liner intantiation
            services.AddDefaultBlazorAuth0Authentication(Configuration.GetSection("Auth0").Get<ClientOptions>());

            // Policy based authorization, learn more here: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("read:weather_forecast", policy => policy.RequireClaim("permissions", "read:weather_forecast"));
                options.AddPolicy("execute:increment_counter", policy => policy.RequireClaim("permissions", "execute:increment_counter"));
            });

            services.AddSingleton<WeatherForecastService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Add Cookies, Authentication, Authorization and Blazor.Auth0 middlewares
            app.UseBlazorAuth0();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
