using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components;
using Blazor.Auth0.Examples.BuiltInAuth.ServerSide.Data;

namespace Blazor.Auth0.Examples.BuiltInAuth.ServerSide
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
            services.AddServerSideBlazor();

            services.AddScoped<HttpClient>();

            services.Configure<Blazor.Auth0.Shared.Models.ClientSettings>(Configuration.GetSection("Auth0"));
            services.AddScoped(resolver => resolver.GetRequiredService<IOptions<Blazor.Auth0.Shared.Models.ClientSettings>>().Value);

            services.AddScoped<Blazor.Auth0.ServerSide.Authentication.AuthenticationService>();

            services.AddScoped<AuthenticationStateProvider, Blazor.Auth0.ServerSide.Authentication.AuthenticationStateProvider>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("read:weather_forecast", policy => policy.RequireClaim("permission:read:weather_forecast"));
                options.AddPolicy("execute:increment_counter", policy => policy.RequireClaim("permission:execute:increment_counter"));
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
