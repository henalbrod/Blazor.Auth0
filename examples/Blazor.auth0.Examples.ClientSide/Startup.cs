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
            services.AddSingleton<Auth0.Authentication.AuthenticationService>();
        }

        public void Configure(IComponentsApplicationBuilder app, IUriHelper uriHelper)
        {

            var authenticationService = app.Services.GetService<Auth0.Authentication.AuthenticationService>();

            // OPTIONAL: Uncomment following line to force the user to be authenticated
            //authenticationService.LoginRequired = true;

            // REQUIRED: Indicate the Auth0's tenant & client information
            authenticationService.Auth0Settings = new Auth0.Models.Auth0Settings()
            {
                Domain = "[AUTH0_DOMAIN]",
                ClientId = "[AUTH0_CLIENT_ID]",
                // OPTIONAL:  Uncomment following line to redirect always to "/" after user authentication, otherwise RedirectUri will be the current path:
                RedirectUri = new Uri(uriHelper.GetAbsoluteUri()).GetLeftPart(System.UriPartial.Authority),
                Scope = "openid profile email"
            };

            // REQUIRED: Initializes the service and validates user's session state.
            authenticationService.ValidateSession();

            app.AddComponent<App>("app");
        }
    }
}
