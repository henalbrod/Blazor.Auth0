using Blazor.Auth0.Shared.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Auth0.ServerSide.Authentication
{
    public class AuthenticationStateProvider : Microsoft.AspNetCore.Components.AuthenticationStateProvider
    {

        private AuthenticationService authenticationService { get; set; }

        public AuthenticationStateProvider(AuthenticationService authenticationServiceBase)
        {

            authenticationService = authenticationServiceBase ?? throw new ArgumentNullException(nameof(authenticationServiceBase));

            authenticationService.OnSessionStateChanged += (a, b) =>
            {
                base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            };

        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return authenticationService.GetAuthenticationStateAsync();
        }

    }

}
