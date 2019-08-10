using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Auth0.Examples.ClientSide
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped((sp) =>
            {
                return new Auth0.Shared.Models.ClientSettings()
                {
                    Auth0Domain = "[Auth0_Domain]",
                    Auth0ClientId = "[Auth0_Client_Id]",
                    //// Redirection to home can be forced uncommenting the following line, this setting primes over Auth0RedirectUri
                    // RedirectAlwaysToHome = true,
                    //// Uncomment following line to force the user to be authenticated
                    // LoginRequired = true
                };
            });

            services.AddScoped<Blazor.Auth0.ClientSide.Authentication.AuthenticationService>();

        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
