using Microsoft.AspNetCore.Components.Builder;
using Microsoft.AspNetCore.Components.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Blazor.auth0.Examples.ClientSide
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddScoped((sp) =>
            {
                return new Auth0.Models.ClientSettings()
                {
                    Auth0Domain = "blazor-demo.auth0.com",
                    Auth0ClientId = "ZTMQoX1IpWJoDxW74PXMc9XNGcy1blYZ",
                     Auth0Audience = "https://blazor-demo.com",
                    //// Redirection to home can be forced uncommenting the following line, this setting primes over Auth0RedirectUri
                    // RedirectAlwaysToHome = true,
                    //// Uncomment following line to force the user to be authenticated
                    // LoginRequired = true
                };
            });

            services.AddScoped<Auth0.Authentication.AuthenticationService>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
