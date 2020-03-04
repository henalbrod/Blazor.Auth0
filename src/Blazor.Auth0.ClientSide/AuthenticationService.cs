// <copyright file="AuthenticationService.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Authentication;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Timers;
    using Blazor.Auth0.ClientSide.Properties;
    using Blazor.Auth0.Models;
    using Blazor.Auth0.Models.Enumerations;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using Timer = System.Timers.Timer;

    /// <inheritdoc/>
    public class AuthenticationService : IAuthenticationService, IDisposable
    {
        private readonly ClientOptions clientOptions;
        private readonly NavigationManager navigationManager;
        private readonly HttpClient httpClient;
        private readonly IJSRuntime jsRuntime;

        private readonly ILogger logger;
        private readonly DotNetObjectReference<AuthenticationService> dotnetObjectRef;

        private SessionAuthorizationTransaction sessionAuthorizationTransaction;
        private Timer logOutTimer;
        private SessionStates sessionState = SessionStates.Undefined;

        /// <inheritdoc/>
        public event EventHandler<SessionStates> SessionStateChangedEvent;

        /// <summary>
        /// The event fired just before staring a silent login.
        /// </summary>
        public event EventHandler<bool> BeforeSilentLoginEvent;

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

        private bool RequiresNonce => this.clientOptions.ResponseType == ResponseTypes.IdToken || this.clientOptions.ResponseType == ResponseTypes.TokenAndIdToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="logger">A <see cref="ILogger"/> param.</param>
        /// <param name="componentContext">A <see cref="IComponentContext"/> param.</param>
        /// <param name="httpClient">A <see cref="HttpClient"/> param.</param>
        /// <param name="jsRuntime">A <see cref="IJSRuntime"/> param.</param>
        /// <param name="navigationManager">A <see cref="NavigationManager"/> param.</param>
        /// <param name="options">A <see cref="ClientOptions"/> param.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "I like this best ;)")]
        public AuthenticationService(ILogger<AuthenticationService> logger, HttpClient httpClient, IJSRuntime jsRuntime, NavigationManager navigationManager, ClientOptions options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
            this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            this.clientOptions = options ?? throw new ArgumentNullException(nameof(options));

            this.dotnetObjectRef = DotNetObjectReference.Create(this);

            Task.Run(async () =>
            {
                // Ugly but necesary :\
                await this.jsRuntime.InvokeVoidAsync("window.eval", Resources.ClientSideJs);
                await this.ValidateSession().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Stops the next Silent Login iterarion.
        /// </summary>
        public void StopSilentLogin()
        {
            this.logOutTimer?.Stop();
        }

        /// <inheritdoc/>
        public async Task Authorize()
        {
            AuthorizeOptions options = this.BuildAuthorizeOptions();

            if (this.clientOptions.LoginMode == LoginModes.Popup)
            {
                await Authentication.AuthorizePopup(this.jsRuntime, this.dotnetObjectRef, this.navigationManager, options).ConfigureAwait(false);
            }
            else
            {
                await Authentication.Authorize(this.jsRuntime, this.navigationManager, options).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task LogOut()
        {
            await this.LogOut(null).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task LogOut(string redirectUri)
        {
            string logoutUrl = CommonAuthentication.BuildLogoutUrl(this.clientOptions.Domain, this.clientOptions.ClientId, redirectUri);

            await this.jsRuntime.InvokeAsync<object>($"{Resources.InteropElementName}.logOut", logoutUrl).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(redirectUri))
            {
                this.navigationManager.NavigateTo(redirectUri);
            }
            else if (this.clientOptions.RequireAuthenticatedUser)
            {
                await this.Authorize().ConfigureAwait(false);
            }
            else
            {
                // There's no redirectUri and an authenticated user is not required
            }

            if (this.clientOptions.RequireAuthenticatedUser)
            {
                await Task.Delay(3000).ConfigureAwait(false);
            }

            this.ClearSession();
        }

        /// <inheritdoc/>
        public Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            GenericIdentity identity = null;

            if (this.SessionState == SessionStates.Active)
            {
                identity = new GenericIdentity(this.User?.Name ?? string.Empty, "JWT");

                if (!string.IsNullOrEmpty(this.User.Sub?.Trim()))
                {
                    identity.AddClaim(new Claim("sub", this.User.Sub));
                }

                if (!string.IsNullOrEmpty(this.User.Name?.Trim()))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, this.User.Name));
                }

                if (!string.IsNullOrEmpty(this.User.GivenName?.Trim()))
                {
                    identity.AddClaim(new Claim("given_name", this.User.GivenName));
                }

                if (!string.IsNullOrEmpty(this.User.FamilyName?.Trim()))
                {
                    identity.AddClaim(new Claim("family_name", this.User.FamilyName));
                }

                if (!string.IsNullOrEmpty(this.User.MiddleName?.Trim()))
                {
                    identity.AddClaim(new Claim("middle_name", this.User.MiddleName));
                }

                if (!string.IsNullOrEmpty(this.User.Nickname?.Trim()))
                {
                    identity.AddClaim(new Claim("nickname", this.User.Nickname));
                }

                if (!string.IsNullOrEmpty(this.User.PreferredUsername?.Trim()))
                {
                    identity.AddClaim(new Claim("preferred_username", this.User.PreferredUsername));
                }

                if (!string.IsNullOrEmpty(this.User.Profile?.Trim()))
                {
                    identity.AddClaim(new Claim("profile", this.User.Profile));
                }

                if (!string.IsNullOrEmpty(this.User.Picture?.Trim()))
                {
                    identity.AddClaim(new Claim("picture", this.User.Picture));
                }

                if (!string.IsNullOrEmpty(this.User.Website?.Trim()))
                {
                    identity.AddClaim(new Claim("website", this.User.Website));
                }

                if (!string.IsNullOrEmpty(this.User.Email?.Trim()))
                {
                    identity.AddClaim(new Claim("email", this.User.Email));
                }

                identity.AddClaim(new Claim("email_verified", this.User.EmailVerified.ToString()));

                if (!string.IsNullOrEmpty(this.User.Gender?.Trim()))
                {
                    identity.AddClaim(new Claim("gender", this.User.Gender));
                }

                if (!string.IsNullOrEmpty(this.User.Birthdate?.Trim()))
                {
                    identity.AddClaim(new Claim("birthdate", this.User.Birthdate));
                }

                if (!string.IsNullOrEmpty(this.User.Zoneinfo?.Trim()))
                {
                    identity.AddClaim(new Claim("zoneinfo", this.User.Zoneinfo));
                }

                if (!string.IsNullOrEmpty(this.User.Locale?.Trim()))
                {
                    identity.AddClaim(new Claim("locale", this.User.Locale));
                }

                if (!string.IsNullOrEmpty(this.User.PhoneNumber?.Trim()))
                {
                    identity.AddClaim(new Claim("phone_number", this.User.PhoneNumber));
                }

                identity.AddClaim(new Claim("phone_number_verified", this.User.PhoneNumberVerified.ToString()));

                if (!string.IsNullOrEmpty(this.User.Address?.Trim()))
                {
                    identity.AddClaim(new Claim("address", this.User.Address));
                }

                identity.AddClaim(new Claim("updated_at", this.User.UpdatedAt.ToString()));

                identity.AddClaims(this.User.CustomClaims.Select(customClaim => new Claim(customClaim.Key, customClaim.Value.GetRawText(), customClaim.Value.ValueKind.ToString())));

                identity.AddClaims(this.User.Permissions.Select(permission => new Claim("permissions", permission, "permissions")));
            }
            else
            {
                identity = new GenericIdentity(string.Empty, "JWT");
            }

            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        /// <inheritdoc/>
        public async Task ValidateSession()
        {
            await this.ValidateSession(this.navigationManager.Uri).ConfigureAwait(false);
        }

        [JSInvokable]
        public async Task ValidateSession(string path)
        {
            // TODO: Add validation such as same host and similars

            // Let's validate the hash
            Uri absoluteUri = this.navigationManager.ToAbsoluteUri(path);

            ParsedHash parsedHash = Authentication.ParseHash(new ParseHashOptions
            {
                ResponseType = this.clientOptions.ResponseType,
                AbsoluteUri = absoluteUri,
            });

            // No hash found?!
            if (parsedHash == null)
            {
                // Should we keep the session alive?
                if (this.clientOptions.SlidingExpiration || this.clientOptions.RequireAuthenticatedUser)
                {
                    await this.SilentLogin().ConfigureAwait(false);
                }
                else
                {
                    await this.LogOut().ConfigureAwait(false);

                    this.ClearSession();
                }
            }
            else
            {
                // We have a valid hash parameter collection, let's validate the authorization response
                await this.HandleAuthorizationResponseAsync(new AuthorizationResponse
                {
                    AccessToken = parsedHash.AccessToken,
                    Code = parsedHash.Code,
                    Error = parsedHash.Error,
                    ErrorDescription = parsedHash.ErrorDescription,
                    ExpiresIn = 15,
                    IdToken = parsedHash.IdToken,
                    IsTrusted = false,
                    Origin = absoluteUri.Authority,
                    Scope = string.Empty,
                    State = parsedHash.State,
                    TokenType = "bearer", // TODO: Improve this validation
                    Type = nameof(ResponseModes.Query), // TODO: Improve this validation
                }).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        [JSInvokable]
        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        public async Task HandleAuthorizationResponseAsync(AuthorizationResponse authorizationResponse)
        {
            try
            {
                this.sessionAuthorizationTransaction = await TransactionManager.GetStoredTransactionAsync(this.jsRuntime, this.clientOptions, authorizationResponse.State).ConfigureAwait(false);

                Authentication.ValidateAuthorizationResponse(authorizationResponse, this.clientOptions.Domain, this.sessionAuthorizationTransaction?.State);

                SessionInfo tempSessionInfo = await this.GetSessionInfoAsync(authorizationResponse).ConfigureAwait(false);

                UserInfo tempIdTokenInfo = await this.GetUserAsync(tempSessionInfo.AccessToken, tempSessionInfo.IdToken).ConfigureAwait(false);

                this.ValidateIdToken(tempIdTokenInfo, authorizationResponse.AccessToken);

                this.InitiateUserSession(tempIdTokenInfo, tempSessionInfo);

                this.ScheduleLogOut();
            }
            catch (AuthenticationException ex)
            {
                await this.OnLoginRequestValidationError(authorizationResponse.Error, ex.Message).ConfigureAwait(false);
            }
            finally
            {
                this.RedirectToHome();
            }
        }

        private void RedirectToHome()
        {
            Uri abosulteUri = new Uri(this.navigationManager.Uri);

            this.sessionAuthorizationTransaction = null;

            // Redirect to home (removing the hash)
            this.navigationManager.NavigateTo(abosulteUri.GetLeftPart(UriPartial.Path));
        }

        private async Task<SessionInfo> GetSessionInfoAsync(AuthorizationResponse authorizationResponse)
        {
            if (authorizationResponse is null)
            {
                throw new ArgumentNullException(nameof(authorizationResponse));
            }

            if (this.clientOptions.ResponseType == ResponseTypes.Code)
            {
                return await this.GetSessionInfoAsync(authorizationResponse.Code).ConfigureAwait(false);
            }

            return new SessionInfo
            {
                AccessToken = authorizationResponse.AccessToken,
                ExpiresIn = authorizationResponse.ExpiresIn,
                IdToken = authorizationResponse.IdToken,
                Scope = authorizationResponse.Scope,
                TokenType = authorizationResponse.TokenType,
            };
        }

        private async Task<SessionInfo> GetSessionInfoAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(code));
            }

            return await Authentication.GetAccessToken(
                    this.httpClient,
                    this.clientOptions.Domain,
                    this.clientOptions.ClientId,
                    code,
                    audience: this.clientOptions.Audience,
                    codeVerifier: this.sessionAuthorizationTransaction?.CodeVerifier,
                    secret: this.clientOptions.ClientSecret,
                    redirectUri: this.sessionAuthorizationTransaction?.RedirectUri)
                .ConfigureAwait(false);
        }

        private async Task<UserInfo> GetUserAsync(string accessToken, string idToken = null)
        {
            if (!string.IsNullOrEmpty(idToken) && (this.RequiresNonce || this.clientOptions.GetUserInfoFromIdToken))
            {
                return CommonAuthentication.DecodeTokenPayload<UserInfo>(idToken);
            }
            else
            {
                // In case we're not getting the id_token from the message response or GetUserInfoFromIdToken is set to false try to get it from Auth0's API
                return await CommonAuthentication.UserInfo(this.httpClient, this.clientOptions.Domain, accessToken).ConfigureAwait(false);
            }
        }

        private void ValidateIdToken(UserInfo idTokenInfo, string accessToken)
        {
            if (this.RequiresNonce)
            {
                bool? nonceIsValid = idTokenInfo?.Nonce.Replace(' ', '+').Equals(this.sessionAuthorizationTransaction?.Nonce.Replace(' ', '+'));

                if (nonceIsValid.HasValue && !nonceIsValid.Value)
                {
                    throw new AuthenticationException(Resources.InvalidNonceError);
                }

                if (string.IsNullOrEmpty(idTokenInfo?.AtHash))
                {
                    this.logger.LogWarning(Resources.NotAltChashWarning);
                }
                else
                {
                    bool atHashIsValid = Authentication.ValidateAccessTokenHash(idTokenInfo?.AtHash, accessToken);

                    if (!atHashIsValid)
                    {
                        throw new AuthenticationException(Resources.InvalidAccessTokenHashError);
                    }
                }
            }
        }

        private void InitiateUserSession(UserInfo userInfo, SessionInfo sessionInfo)
        {
            if (!string.IsNullOrEmpty(this.clientOptions.Audience) && !string.IsNullOrEmpty(sessionInfo.AccessToken))
            {
                List<string> permissionsList = CommonAuthentication.DecodeTokenPayload<AccessTokenPayload>(sessionInfo.AccessToken).Permissions ?? new List<string>();
                userInfo.Permissions.AddRange(permissionsList);
            }

            this.User = userInfo;

            this.SessionInfo = sessionInfo;

            this.SessionState = SessionStates.Active;
        }

        private async Task OnLoginRequestValidationError(string error, string validationMessage)
        {
            // In case of any error negate the access
            if (!string.IsNullOrEmpty(validationMessage))
            {
                this.ClearSession();

                this.logger.LogError("Login Error: " + validationMessage);

                if (error.ToLowerInvariant() == "login_required" && this.clientOptions.RequireAuthenticatedUser)
                {
                    await this.Authorize().ConfigureAwait(false);
                    System.Threading.Thread.Sleep(30000);
                    this.navigationManager.NavigateTo("/");
                }
            }
        }

        private AuthorizeOptions BuildAuthorizeOptions()
        {
            bool isUsingSecret = !string.IsNullOrEmpty(this.clientOptions.ClientSecret);
            ResponseTypes responseType = isUsingSecret ? ResponseTypes.Code : this.clientOptions.ResponseType;
            ResponseModes responseMode = isUsingSecret ? ResponseModes.Query : this.clientOptions.ResponseMode;
            CodeChallengeMethods codeChallengeMethod = !isUsingSecret && responseType == ResponseTypes.Code ? CodeChallengeMethods.S256 : CodeChallengeMethods.None;
            string codeVerifier = codeChallengeMethod != CodeChallengeMethods.None ? CommonAuthentication.GenerateNonce(this.clientOptions.KeyLength) : null;
            string codeChallenge = codeChallengeMethod != CodeChallengeMethods.None ? Utils.GetSha256(codeVerifier) : null;
            string nonce = CommonAuthentication.GenerateNonce(this.clientOptions.KeyLength);

            return new AuthorizeOptions
            {
                Audience = this.clientOptions.Audience,
                ClientID = this.clientOptions.ClientId,
                CodeChallengeMethod = codeChallengeMethod,
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                Connection = this.clientOptions.Connection,
                Domain = this.clientOptions.Domain,
                Nonce = nonce,
                Realm = this.clientOptions.Realm,
                RedirectUri = this.BuildRedirectUrl(),
                ResponseMode = responseMode,
                ResponseType = responseType,
                Scope = this.clientOptions.Scope,
                State = CommonAuthentication.GenerateNonce(this.clientOptions.KeyLength),
                Namespace = this.clientOptions.Namespace,
                KeyLength = this.clientOptions.KeyLength,
            };
        }

        private void ClearSession()
        {
            this.SessionState = this.clientOptions.RequireAuthenticatedUser ? SessionStates.Undefined : SessionStates.Inactive;
            this.User = null;
            this.SessionInfo = null;
            this.sessionAuthorizationTransaction = null;
            this.logOutTimer?.Stop();
        }

        private async Task SilentLogin()
        {

            this.BeforeSilentLoginEvent?.Invoke(this, false);

            AuthorizeOptions options = this.BuildAuthorizeOptions();
            options.ResponseMode = ResponseModes.Web_Message;

            options = await TransactionManager.Proccess(this.jsRuntime, options).ConfigureAwait(false);

            string authorizeUrl = Authentication.BuildAuthorizeUrl(options);

            await this.jsRuntime.InvokeAsync<object>($"{Resources.InteropElementName}.drawIframe", this.dotnetObjectRef, $"{authorizeUrl}&prompt=none").ConfigureAwait(false);
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
                        await this.SilentLogin().ConfigureAwait(false);
                        return;
                    }

                    await this.LogOut().ConfigureAwait(false);

                    this.ClearSession();
                };
            }

            this.logOutTimer.Interval = (this.SessionInfo.ExpiresIn - 5) * 1000;

            this.logOutTimer.Start();
        }

        private string BuildRedirectUrl()
        {
            Uri abosulteUri = new Uri(this.navigationManager.Uri);
            string uri = !string.IsNullOrEmpty(this.clientOptions.RedirectUri) ? this.clientOptions.RedirectUri : this.clientOptions.RedirectAlwaysToHome ? abosulteUri.GetLeftPart(UriPartial.Authority) : abosulteUri.AbsoluteUri;

            return !string.IsNullOrEmpty(this.clientOptions.RedirectUri) && !this.clientOptions.RedirectAlwaysToHome ? this.clientOptions.RedirectUri : uri;
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
                    this.dotnetObjectRef.Dispose();
                    this.httpClient.Dispose();
                    this.logOutTimer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.disposedValue = true;
            }
        }

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