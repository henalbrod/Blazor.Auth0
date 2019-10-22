// <copyright file="AuthenticationService.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Web;
    using Blazor.Auth0.Models;
    using Blazor.Auth0.Models.Enumerations;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Timer = System.Timers.Timer;

    /// <inheritdoc/>
    public class AuthenticationService : IAuthenticationService, IDisposable
    {
        private readonly ClientOptions clientOptions;
        private readonly NavigationManager navigationManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly HttpClient httpClient;
        private SessionStates sessionState = SessionStates.Undefined;
        private ClaimsPrincipal claimsPrincipal;

        private Timer logOutTimer;

        /// <inheritdoc/>
        public event EventHandler<SessionStates> SessionStateChangedEvent;

        /// <inheritdoc/>
        public UserInfo User { get; private set; }

        /// <inheritdoc/>
        public SessionStates SessionState
        {
            get => this.sessionState;
            private set
            {
                if (value != this.sessionState)
                {
                    this.sessionState = value;
                    this.SessionStateChangedEvent?.Invoke(this, this.SessionState);
                }
            }
        }

        /// <inheritdoc/>
        public SessionInfo SessionInfo { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance.</param>
        /// /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> instance.</param>
        /// <param name="navigationManager">The <see cref="NavigationManager"/> instance.</param>
        /// <param name="options">The <see cref="ClientOptions"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I like it best ;)")]
        public AuthenticationService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            NavigationManager navigationManager,
            ClientOptions options)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            this.clientOptions = options ?? throw new ArgumentNullException(nameof(options));

            this.claimsPrincipal = this.httpContextAccessor.HttpContext.User;

            if (this.claimsPrincipal.Identity.IsAuthenticated)
            {
                Task.Run(async () =>
                {
                    string refreshToken = await this.httpContextAccessor.HttpContext.GetTokenAsync(this.clientOptions.ClaimsIssuer, "refresh_token").ConfigureAwait(false);
                    string accessToken = await this.httpContextAccessor.HttpContext.GetTokenAsync(this.clientOptions.ClaimsIssuer, "access_token").ConfigureAwait(false);
                    string idToken = await this.httpContextAccessor.HttpContext.GetTokenAsync(this.clientOptions.ClaimsIssuer, "id_token").ConfigureAwait(false);
                    string expiresIn = await this.httpContextAccessor.HttpContext.GetTokenAsync(this.clientOptions.ClaimsIssuer, "expires_in").ConfigureAwait(false);

                    await this.HandleAuthorizationResponseAsync(new AuthorizationResponse
                    {
                        AccessToken = accessToken,
                        IdToken = idToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = string.IsNullOrEmpty(expiresIn) ? 0 : int.Parse(expiresIn),
                    }).ConfigureAwait(false);
                });
            }
            else
            {
                this.sessionState = SessionStates.Inactive;
            }
        }

        /// <inheritdoc/>
        public Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsPrincipal user;

            if (this.SessionState == SessionStates.Active)
            {
                user = this.claimsPrincipal;
            }
            else
            {
                user = new ClaimsPrincipal(new GenericIdentity(string.Empty, "JWT"));
            }

            return Task.FromResult(new AuthenticationState(user));
        }

        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        public async Task HandleAuthorizationResponseAsync(AuthorizationResponse authorizationResponse)
        {
            if (authorizationResponse is null)
            {
                throw new ArgumentNullException(nameof(authorizationResponse));
            }

            SessionInfo tempSessionInfo = authorizationResponse;

            UserInfo tempIdTokenInfo = CommonAuthentication.DecodeTokenPayload<UserInfo>(tempSessionInfo.IdToken);

            this.InitiateUserSession(tempIdTokenInfo, tempSessionInfo);

            this.ScheduleLogOut();
        }

        /// <inheritdoc/>
        public Task ValidateSession()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Authorize()
        {
            this.navigationManager.NavigateTo(this.clientOptions.AuthorizePath, true);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task LogOut()
        {
            await this.LogOut(null).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task LogOut(string redirectUri)
        {
            this.ClearSession();

            redirectUri ??= this.BuildRedirectUrl();

            Uri abosulteUri = new Uri(this.navigationManager.Uri);

            var path = new PathString(this.clientOptions.RemoteSignOutPath);
            var query = new QueryString();

            query = query.Add("redirect_uri", redirectUri);

            var uri = new UriBuilder
            {
                Scheme = abosulteUri.Scheme,
                Host = abosulteUri.Host,
                Port = abosulteUri.Port,
                Path = path,
                Query = query.ToUriComponent(),
            };

            this.navigationManager.NavigateTo(uri.Uri.AbsoluteUri, true);
        }

        private void InitiateUserSession(UserInfo userInfo, SessionInfo sessionInfo)
        {
            if (!string.IsNullOrEmpty(this.clientOptions.Audience) && !string.IsNullOrEmpty(sessionInfo.AccessToken))
            {
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

                AccessTokenPayload parsedAccessToken = CommonAuthentication.DecodeTokenPayload<AccessTokenPayload>(sessionInfo.AccessToken);
                userInfo.Permissions.AddRange(parsedAccessToken.Permissions ?? new List<string>());
                sessionInfo.ExpiresIn = (int)(parsedAccessToken.Exp - epoch);
            }

            this.User = userInfo;

            this.SessionInfo = sessionInfo;

            this.SessionState = SessionStates.Active;
        }

        private string BuildRedirectUrl()
        {
            Uri abosulteUri = new Uri(this.navigationManager.Uri);

            string uri = !string.IsNullOrEmpty(this.clientOptions.RedirectUri) ? this.clientOptions.RedirectUri : this.clientOptions.RedirectAlwaysToHome ? abosulteUri.GetLeftPart(UriPartial.Authority) : abosulteUri.AbsoluteUri;

            return !string.IsNullOrEmpty(this.clientOptions.RedirectUri) && !this.clientOptions.RedirectAlwaysToHome ? this.clientOptions.RedirectUri : uri;
        }

        private void ScheduleLogOut()
        {
            this.logOutTimer?.Stop();

            if (this.logOutTimer == null)
            {
                this.logOutTimer = new Timer();
                this.logOutTimer.Elapsed += async (object source, ElapsedEventArgs e) =>
                {
                    this.logOutTimer.Stop();

                    if (this.clientOptions.SlidingExpiration)
                    {
                        AuthorizationResponse refreshTokenResult = await Authentication.RefreshToken(this.httpClient, this.clientOptions.Domain, this.clientOptions.Audience, this.clientOptions.ClientId, this.clientOptions.ClientSecret, this.SessionInfo.RefreshToken).ConfigureAwait(false);

                        if (string.IsNullOrEmpty(refreshTokenResult.Error))
                        {
                            refreshTokenResult.IsTrusted = true;
                            refreshTokenResult.RefreshToken = this.SessionInfo.RefreshToken;

                            await this.HandleAuthorizationResponseAsync(refreshTokenResult).ConfigureAwait(false);
                            return;
                        }
                    }

                    await this.LogOut().ConfigureAwait(false);

                    this.ClearSession();
                };
            }

            this.logOutTimer.Interval = this.SessionInfo.ExpiresIn * 1000;

            this.logOutTimer.Start();
        }

        private void ClearSession()
        {
            this.SessionState = this.clientOptions.RequireAuthenticatedUser ? SessionStates.Undefined : SessionStates.Inactive;
            this.User = null;
            this.claimsPrincipal = null;
            this.SessionInfo = null;
            this.logOutTimer?.Stop();
            this.logOutTimer?.Dispose();
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).                    
                    this.logOutTimer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AuthenticationService()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}