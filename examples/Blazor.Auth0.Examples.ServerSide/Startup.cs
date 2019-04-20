using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Blazor.Auth0.Examples.ServerSide.Data;
using System.Net.Http;

namespace Blazor.Auth0.Examples.ServerSide
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {        

            services.AddRazorPages();
            services.AddServerSideBlazor();            

            services.AddScoped<HttpClient>();
            services.AddScoped((sp) =>
            {
                return new Auth0.Shared.Models.ClientSettings()
                {
                    Auth0Domain = "blazor-demo.auth0.com",
                    Auth0ClientId = "ZTMQoX1IpWJoDxW74PXMc9XNGcy1blYZ",
                    //// Redirection to home can be forced uncommenting the following line, this setting primes over Auth0RedirectUri
                    // RedirectAlwaysToHome = true,
                    //// Uncomment following line to force the user to be authenticated
                    // LoginRequired = true
                };
            });
            services.AddScoped<Auth0.ServerSide.Authentication.AuthenticationService>();
            
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
