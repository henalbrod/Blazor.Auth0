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
                var uriHelper = sp.GetRequiredService<IUriHelper>();
                return new Auth0.Models.ClientSettings()
                {
                    Auth0Domain = "blazor-demo.auth0.com",
                    Auth0ClientId = "ZTMQoX1IpWJoDxW74PXMc9XNGcy1blYZ",
                    Auth0Scope = "openid profile email",
                    Auth0Audience = "https://blazor-demo.com",
                    // Uncomment following line to redirect always to an specific URL after user authentication, otherwise Auth0RedirectUri will be the current path:
                    // Auth0RedirectUri = "http://localhost:62702/counter",
                    // Redirection to home can be forced uncommenting the following line, this setting primes over Auth0RedirectUri
                    // RedirectAlwaysToHome = true,
                    // Uncomment following line to force the user to be authenticated
                    // LoginRequired = true
                     GetUserInfoFromIdToken=true
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
