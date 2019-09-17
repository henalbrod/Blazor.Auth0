// <copyright file="AuthenticationStateProvider.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.Authorization;

    /// <inheritdoc/>
    public class AuthenticationStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        private readonly IAuthenticationService authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationStateProvider"/> class.
        /// </summary>
        /// <param name="authenticationService">A <see cref="IAuthenticationService"/> instance.</param>
        public AuthenticationStateProvider(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));

            this.authenticationService.SessionStateChangedEvent += (a, b) =>
            {
                this.NotifyAuthenticationStateChanged(this.GetAuthenticationStateAsync());
            };
        }

        /// <inheritdoc/>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return this.authenticationService.GetAuthenticationStateAsync();
        }
    }
}
