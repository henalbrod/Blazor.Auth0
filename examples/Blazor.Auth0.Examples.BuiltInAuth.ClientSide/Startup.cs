using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Auth0.Examples.BuiltInAuth.ClientSide
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped((sp) =>
            {
                return new Blazor.Auth0.Shared.Models.ClientSettings()
                {
                    Auth0Domain = "[Auth0_Domain]",
                    Auth0ClientId = "[Auth0_Client_Id]",
                    Auth0Audience = "[Auth0_Audience]"
                };
            });

            services.AddScoped<Blazor.Auth0.ClientSide.Authentication.AuthenticationService>();

            services.AddScoped<AuthenticationStateProvider, Blazor.Auth0.ClientSide.Authentication.AuthenticationStateProvider>();

            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("read:weather_forecast", policy => policy.RequireClaim("permission:read:weather_forecast"));
                options.AddPolicy("execute:increment_counter", policy => policy.RequireClaim("permission:execute:increment_counter"));
            });

        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }

    }

}
