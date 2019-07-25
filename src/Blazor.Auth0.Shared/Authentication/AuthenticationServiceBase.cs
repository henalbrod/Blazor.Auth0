using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Blazor.Auth0.Shared.Models.Enumerations;
using Blazor.Auth0.Shared.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace Blazor.Auth0.Shared.Authentication
{
    public abstract class AuthenticationServiceBase
    {
        public EventHandler<SessionStates> OnSessionStateChanged { get; set; }
        public UserInfoDto User { get; private set; }
        public SessionStates SessionState { get; private set; }
        public SessionInfo SessionInfo { get; private set; }

        protected readonly ClientSettings clientSettings;
        protected readonly IUriHelper uriHelperService;
        protected readonly HttpClient httpClientService;
        protected readonly IJSRuntime jsRuntimeService;

        protected string latestAuthorizeUrlCodeChallenge;
        protected string latestAuthorizeUrlState;
        protected string latestAuthorizeUrRedirectUri;

        protected Timer nextSilentLoginTimer;
        protected Timer injectJavascriptTimer;

        protected IComponentContext componentContextService;

        private struct Auth0GetAccessTokenResponseDto
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string id_token { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
            public string token_type { get; set; }
        }

        #region Hack to fix https://github.com/aspnet/AspNetCore/issues/11159

        public static object CreateDotNetObjectRefSyncObj = new object();

        protected DotNetObjectRef<T> CreateDotNetObjectRef<T>(T value) where T : class
        {
            lock (CreateDotNetObjectRefSyncObj)
            {
                JSRuntime.SetCurrentJSRuntime(jsRuntimeService);
                return DotNetObjectRef.Create(value);
            }
        }

        #endregion

        public AuthenticationServiceBase(IComponentContext componentContext, HttpClient httpClient, IJSRuntime jsRuntime, IUriHelper uriHelper, ClientSettings settings)
        {
            componentContextService = componentContext ?? throw new ArgumentNullException(nameof(componentContext)); ;
            httpClientService = httpClient ?? throw new ArgumentNullException(nameof(httpClient)); ;
            jsRuntimeService = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime)); ;
            uriHelperService = uriHelper ?? throw new ArgumentNullException(nameof(uriHelper)); ;
            clientSettings = settings ?? throw new ArgumentNullException(nameof(settings));

            injectJavascriptTimer = new Timer(50);
            injectJavascriptTimer.Elapsed += async (Object source, ElapsedEventArgs e) => await InjectJavascript();
            injectJavascriptTimer.Start();
        }

        public string BuildAuthorizeUrl()
        {

            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var responseType = string.Empty;

            latestAuthorizeUrlCodeChallenge = GenerateNonce();
            latestAuthorizeUrlState = GenerateNonce();
            var nonce = GenerateNonce();

            Task.Run(async () => await jsRuntimeService.InvokeAsync<object>("setNonce", nonce)).ConfigureAwait(false);

            if (clientSettings.RedirectAlwaysToHome)
            {
                latestAuthorizeUrRedirectUri = abosulteUri.GetLeftPart(UriPartial.Authority);
            }
            if (!clientSettings.RedirectAlwaysToHome)
            {
                latestAuthorizeUrRedirectUri = !string.IsNullOrEmpty(clientSettings.Auth0RedirectUri) ? clientSettings.Auth0RedirectUri : abosulteUri.GetLeftPart(UriPartial.Path);
            }

            switch (clientSettings.AuthenticationGrant)
            {
                case AuthenticationGrantTypes.implicit_grant:
                    responseType = "token id_token";
                    break;
                default:
                    responseType = "code";
                    break;
            }

            return $"https://{clientSettings.Auth0Domain}/authorize?" +
                      $"response_type={responseType}" +
                      "&code_challenge_method=S256" +
                      $"&code_challenge={latestAuthorizeUrlCodeChallenge}" +
                      $"&state={latestAuthorizeUrlState}" +
                      $"&nonce={nonce}" +
                      $"&client_id={clientSettings.Auth0ClientId}" +
                      $"&scope={clientSettings.Auth0Scope.Replace(" ", "%20")}" +
                      (!string.IsNullOrEmpty(clientSettings.Auth0Connection) ? "&connection=" + clientSettings.Auth0Connection : "") +
                      (!string.IsNullOrEmpty(clientSettings.Auth0Audience) ? "&audience=" + clientSettings.Auth0Audience : "") +
                      $"&redirect_uri={latestAuthorizeUrRedirectUri}";

        }
        public string BuildLogoutUrl()
        {
            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var host = abosulteUri.GetLeftPart(UriPartial.Authority);
            return $"https://{clientSettings.Auth0Domain}/v2/logout?" +
                   $"client_id={clientSettings.Auth0ClientId}" +
                   $"&returnTo={(clientSettings.RedirectAlwaysToHome ? host : string.IsNullOrEmpty(clientSettings.Auth0RedirectUri) ? host : clientSettings.Auth0RedirectUri)}";

        }
        public void Authorize()
        {
            uriHelperService.NavigateTo(BuildAuthorizeUrl());
        }
        public void LogOut()
        {
            ClearSession();
            uriHelperService.NavigateTo(BuildLogoutUrl());
        }
        public async Task ValidateSession()
        {
            //TODO: Replace hacky CreateDotNetObjectRef call for DotNetObjectRef.Create(this)
            await jsRuntimeService.InvokeAsync<object>("drawAuth0Iframe", CreateDotNetObjectRef(this), $"{BuildAuthorizeUrl()}&response_mode=web_message&prompt=none").ConfigureAwait(false);
        }
        /// <summary>
        /// Makes a call to the /userinfo endpoint and returns the user profile
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<UserInfoDto> UserInfo(string accessToken)
        {

            httpClientService.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClientService.GetAsync($@"https://{clientSettings.Auth0Domain}/userinfo");

            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

                var claims = JsonSerializer.Parse<Dictionary<string, object>>(responseText);

                return GetUserInfoFromClaims(claims);

            }

            return null;

        }

        public Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            if (SessionState == SessionStates.Active)
            {

                var identity = new GenericIdentity(User?.Name ?? string.Empty, "Auth0 User");


                if (!string.IsNullOrEmpty(User.Sub?.Trim()))
                {
                    identity.AddClaim(new Claim("sub", User.Sub));
                }

                if (!string.IsNullOrEmpty(User.Name?.Trim()))
                {
                    identity.AddClaim(new Claim("name", User.Name));
                }

                if (!string.IsNullOrEmpty(User.GivenName?.Trim()))
                {
                    identity.AddClaim(new Claim("given_name", User.GivenName));
                }

                if (!string.IsNullOrEmpty(User.FamilyName?.Trim()))
                {
                    identity.AddClaim(new Claim("family_name", User.FamilyName));
                }

                if (!string.IsNullOrEmpty(User.MiddleName?.Trim()))
                {
                    identity.AddClaim(new Claim("middle_name", User.MiddleName));
                }

                if (!string.IsNullOrEmpty(User.Nickname?.Trim()))
                {
                    identity.AddClaim(new Claim("nickname", User.Nickname));
                }

                if (!string.IsNullOrEmpty(User.PreferredUsername?.Trim()))
                {
                    identity.AddClaim(new Claim("preferred_username", User.PreferredUsername));
                }

                if (!string.IsNullOrEmpty(User.Profile?.Trim()))
                {
                    identity.AddClaim(new Claim("profile", User.Profile));
                }

                if (!string.IsNullOrEmpty(User.Picture?.Trim()))
                {
                    identity.AddClaim(new Claim("picture", User.Picture));
                }

                if (!string.IsNullOrEmpty(User.Website?.Trim()))
                {
                    identity.AddClaim(new Claim("website", User.Website));
                }

                if (!string.IsNullOrEmpty(User.Email?.Trim()))
                {
                    identity.AddClaim(new Claim("email", User.Email));
                }

                identity.AddClaim(new Claim("email_verified", User.EmailVerified.ToString()));

                if (!string.IsNullOrEmpty(User.Gender?.Trim()))
                {
                    identity.AddClaim(new Claim("gender", User.Gender));
                }

                if (!string.IsNullOrEmpty(User.Birthdate?.Trim()))
                {
                    identity.AddClaim(new Claim("birthdate", User.Birthdate));
                }

                if (!string.IsNullOrEmpty(User.Zoneinfo?.Trim()))
                {
                    identity.AddClaim(new Claim("zoneinfo", User.Zoneinfo));
                }

                if (!string.IsNullOrEmpty(User.Locale?.Trim()))
                {
                    identity.AddClaim(new Claim("locale", User.Locale));
                }

                if (!string.IsNullOrEmpty(User.PhoneNumber?.Trim()))
                {
                    identity.AddClaim(new Claim("phone_number", User.PhoneNumber));
                }

                identity.AddClaim(new Claim("phone_number_verified", User.PhoneNumberVerified.ToString()));

                if (!string.IsNullOrEmpty(User.Address?.Trim()))
                {
                    identity.AddClaim(new Claim("address", User.Address));
                }


                identity.AddClaim(new Claim("updated_at", User.UpdatedAt.ToString()));


                foreach (var claim in User.CustomClaims) {

                    if (!string.IsNullOrEmpty(claim.Value?.ToString().Trim()))
                    {
                        identity.AddClaim(new Claim(claim.Key, claim.Value.ToString()));
                    }
                }


                // Try to add permissions claims, this only works when an audience is indicated
                if (!string.IsNullOrEmpty(clientSettings.Auth0Audience)){

                    var accessTokenPayload = DecodeTokenPayload(SessionInfo.AccessToken);

                    if (accessTokenPayload.CustomClaims.ContainsKey("permissions"))
                    {

                        var permissionsClaim = accessTokenPayload.CustomClaims["permissions"].ToString();

                        identity.AddClaim(new Claim("permissions", permissionsClaim));

                        var permisions = JsonSerializer.Parse<List<string>>(permissionsClaim);

                        identity.AddClaims(permisions.Select(x => new Claim($"permission:{x}", "true")));

                    }

                }


                var user = new ClaimsPrincipal(identity);

                return Task.FromResult(new AuthenticationState(user));

            }
            else
            {
                var identity = new GenericIdentity("", "Auth0 User");
                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));

            }

        }

        virtual public async Task HandleAuth0Message(Auth0IframeMessage message)
        {

            var previousSessionState = SessionState;
            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var validationError = ValidateAth0IframeMessage(message);

            SessionInfo sessionInfo = null;

            if (string.IsNullOrEmpty(validationError))
            {

                switch (clientSettings.AuthenticationGrant)
                {
                    case AuthenticationGrantTypes.implicit_grant:

                        sessionInfo = new SessionInfo()
                        {
                            AccessToken = message.AccessToken,
                            ExpiresIn = message.ExpiresIn,
                            IdToken = message.IdToken,
                            Scope = message.Scope,
                            TokenType = message.TokenType
                        };

                        break;
                    default:

                        sessionInfo = await GetAccessToken(message.Code);

                        break;
                }

                var nonceIsValid = await ValidateNonce(sessionInfo.IdToken);
                var atHashIsValid = ValidateAccessTokenHash(sessionInfo.IdToken, sessionInfo.AccessToken);

                if (!nonceIsValid)
                {
                    validationError = "Invalid Nonce";
                }

                if (!atHashIsValid)
                {
                    validationError = "Invalid at_hash";
                }

            }

            if (string.IsNullOrEmpty(validationError))
            {

                SessionInfo = sessionInfo;

                ScheduleSilentLogin();

                if (clientSettings.GetUserInfoFromIdToken && !string.IsNullOrEmpty(SessionInfo.IdToken))
                {
                    // Decode JWT payload into user info
                    User = DecodeTokenPayload(SessionInfo.IdToken);
                }
                else
                {
                    // In case we're not getting the id_token from the message response or GetUserInfoFromIdToken is set to false try to get it from Auth0's API
                    User = await UserInfo(SessionInfo.AccessToken);
                }

                SessionState = SessionStates.Active;

            }

            if (!string.IsNullOrEmpty(validationError))
            {

                ClearSession();

                Console.WriteLine("Login Error: " + validationError);

                if (message.Error.ToLower() == "login_required" && clientSettings.LoginRequired)
                {
                    uriHelperService.NavigateTo(BuildAuthorizeUrl());
                    System.Threading.Thread.Sleep(30000);
                    uriHelperService.NavigateTo("/");
                }

            }

            if (previousSessionState != SessionState)
            {
                SetIsLoggedIn();
                InvokeOnSessionStateChanged();
            }

            // Redirect to home (removing the hash)
            uriHelperService.NavigateTo(abosulteUri.GetLeftPart(UriPartial.Path));

        }


        private string ValidateAth0IframeMessage(Auth0IframeMessage message)
        {

            string loginError = null;
            var origin = new Uri(message.Origin);

            // Validate Origin
            if (!message.IsTrusted || origin.Authority != clientSettings.Auth0Domain) loginError = "Invalid Origin";

            // Validate Error
            if (loginError == null && !string.IsNullOrEmpty(message.Error))
            {
                switch (message.Error.ToLower())
                {
                    case "login_required":

                        loginError = "Login Required";

                        break;
                    default:
                        loginError = message.ErrorDescription;
                        break;
                }
            }

            // Validate State
            if (loginError == null && !string.IsNullOrEmpty(latestAuthorizeUrlState) ? latestAuthorizeUrlState != message.State.Replace(' ', '+') : false) loginError = "Invalid State";

            return loginError;

        }
        private void ClearSession()
        {
            SessionState = SessionStates.Inactive;
            User = null;
            SessionInfo = null;
            latestAuthorizeUrlCodeChallenge = null;
            latestAuthorizeUrlState = null;
            nextSilentLoginTimer?.Stop();
            nextSilentLoginTimer?.Dispose();
            SetIsLoggedIn();
        }
        /// <summary>
        /// This should be called just after writing SessionState value
        /// </summary>
        private void SetIsLoggedIn()
        {
            Task.Run(async () => await jsRuntimeService.InvokeAsync<object>("setIsLoggedIn", SessionState == SessionStates.Active)).ConfigureAwait(false);
        }

        private async Task<SessionInfo> GetAccessToken(string code)
        {

            HttpContent content = new StringContent(JsonSerializer.ToString(new
            {
                grant_type = "authorization_code",
                client_id = clientSettings.Auth0ClientId,
                code_verifier = latestAuthorizeUrlCodeChallenge,
                code,
                redirect_uri = latestAuthorizeUrRedirectUri
            }), Encoding.UTF8, "application/json");


            HttpResponseMessage httpResponseMessage = await httpClientService.PostAsync($@"https://{clientSettings.Auth0Domain}/oauth/token", content);

            var responseText = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {

                var sessionInfo = JsonSerializer.Parse<Auth0GetAccessTokenResponseDto>(responseText);

                return new SessionInfo()
                {
                    AccessToken = sessionInfo.access_token,
                    ExpiresIn = sessionInfo.expires_in,
                    IdToken = sessionInfo.id_token,
                    RefreshToken = sessionInfo.refresh_token,
                    Scope = sessionInfo.scope,
                    TokenType = sessionInfo.token_type
                };
            }

            return null;

        }
        protected UserInfoDto DecodeTokenPayload(string token)
        {

            string tokenPayload = token.Split('.')[1].Replace('-', '+').Replace('_', '/');

            switch (tokenPayload.Length % 4)
            {
                case 2:
                    tokenPayload += "==";
                    break;
                case 3:
                    tokenPayload += "=";
                    break;
            }

            var bytesArray = Convert.FromBase64String(tokenPayload);
            var decodedString = Encoding.UTF8.GetString(bytesArray, 0, bytesArray.Count());

            var claims = JsonSerializer.Parse<Dictionary<string, object>>(decodedString);

            return GetUserInfoFromClaims(claims);

        }
        private UserInfoDto GetUserInfoFromClaims(Dictionary<string, object> claims)
        {

            var result = new UserInfoDto();

            foreach (var claim in claims)
            {

                switch (claim.Key)
                {
                    case nameof(StandarClaims.address):
                        result.Address = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.birthdate):
                        result.Birthdate = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.email):
                        result.Email = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.email_verified):
                        result.EmailVerified = bool.Parse(claim.Value?.ToString() ?? "false");
                        break;
                    case nameof(StandarClaims.family_name):
                        result.FamilyName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.gender):
                        result.Gender = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.given_name):
                        result.GivenName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.locale):
                        result.Locale = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.middle_name):
                        result.MiddleName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.name):
                        result.Name = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.nickname):
                        result.Nickname = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.phone_number):
                        result.PhoneNumber = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.phone_number_verified):
                        result.PhoneNumberVerified = bool.Parse(claim.Value?.ToString() ?? "false");
                        break;
                    case nameof(StandarClaims.picture):
                        result.Picture = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.preferred_username):
                        result.PreferredUsername = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.profile):
                        result.Profile = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.sub):
                        result.Sub = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.updated_at):
                        result.UpdatedAt = !string.IsNullOrEmpty(claim.Value?.ToString()) ? Convert.ToDateTime(claim.Value?.ToString()) : new DateTime();
                        break;
                    case nameof(StandarClaims.website):
                        result.Website = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.zoneinfo):
                        result.Zoneinfo = claim.Value?.ToString();
                        break;
                    default:
                        result.CustomClaims.Add(claim.Key, claim.Value);
                        break;
                }

            }

            return result;

        }
        private void ScheduleSilentLogin()
        {

            nextSilentLoginTimer?.Stop();

            if (nextSilentLoginTimer == null)
            {
                nextSilentLoginTimer = new Timer();
                nextSilentLoginTimer.Elapsed += async (Object source, ElapsedEventArgs e) =>
                {
                    await ValidateSession();
                };
            }

            nextSilentLoginTimer.Interval = (SessionInfo.ExpiresIn - 5) * 1000;

            nextSilentLoginTimer.Start();

        }
        private void InvokeOnSessionStateChanged()
        {

            if (OnSessionStateChanged != null)
            {
                OnSessionStateChanged.Invoke(this, SessionState);
            }

        }
        private string GenerateNonce()
        {

            string result = "";

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                result = Convert.ToBase64String(tokenData);
            }

            return result;

        }

        virtual protected async Task<bool> ValidateNonce(string idToken)
        {

            var idTokenPayload = DecodeTokenPayload(idToken);
            var nonce = await jsRuntimeService.InvokeAsync<string>("getNonce", null);
            var idTokenNonce = idTokenPayload.CustomClaims["nonce"]?.ToString().Replace(' ', '+');

            var task = jsRuntimeService.InvokeAsync<string>("clearNonce", null).ConfigureAwait(false);

            return nonce.Replace(' ', '+').Equals(idTokenNonce);

        }
        protected bool ValidateAccessTokenHash(string idToken, string accessToken)
        {
            var atHashName = "at_hash";
            var idTokenPayload = DecodeTokenPayload(idToken);

            if (idTokenPayload.CustomClaims.ContainsKey(atHashName))
            {

                var accessTokenHash = string.Empty;

                using (SHA256 mySHA256 = SHA256.Create())
                {

                    byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                    var base64Encoded = Convert.ToBase64String(hashValue.Take(16).ToArray());
                    accessTokenHash = Convert.ToBase64String(hashValue.Take(16).ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');

                }

                return accessTokenHash.Equals(idTokenPayload.CustomClaims["at_hash"].ToString());

            }
            else
            {

                if (clientSettings.AuthenticationGrant == AuthenticationGrantTypes.implicit_grant)
                {
                    Console.WriteLine("WARNING: No at_hash claim was present in the id_token.");
                }

                return true;

            }

        }


        private async Task InjectJavascript()
        {

            injectJavascriptTimer.Stop();

            if (componentContextService.IsConnected)
            {
                var magicString = @"
(function (window, document) {

    window.isLoggedIn = () => localStorage.getItem('isLoggedIn') === 'true';
    window.setIsLoggedIn = (val) => localStorage.setItem('isLoggedIn', val);
    window.getNonce = () => localStorage.getItem('nonce');
    window.setNonce = (val) => localStorage.setItem('nonce', val);
    window.clearNonce = () => localStorage.removeItem('nonce');

    window.drawAuth0Iframe = (instance, src) => {
";

#if DEBUG
                magicString += @"
    console.log('instance: ', instance);
    console.log('src: ', src);";
#endif

                magicString += @"
    let iframe = document.createElement('iframe');
    iframe.setAttribute('src', src);
    iframe.style.display = 'none';
    document.body.appendChild(iframe);
    var messageListener = (msg) => {
        if (msg.data.type == 'authorization_response') {
            window.removeEventListener('message', messageListener);
            instance.invokeMethodAsync('HandleAuth0Message', {
                isTrusted: msg.isTrusted,
                origin: msg.origin,
                type: msg.data.type,
                state: msg.data.response.state,
                error: msg.data.response.error,
                errorDescription: msg.data.response.error_description,
                // Code Grant (Recommended)
                code: msg.data.response.code,
                // Implicit Grant (Legacy)
                accessToken: msg.data.response.access_token,
                idToken: msg.data.response.id_token,
                scope: msg.data.response.scope,
                tokenType: msg.data.response.token_type,
                expiresIn: msg.data.response.expires_in
            }).then((r) => { document.body.removeChild(iframe); });
        }
    };
    window.addEventListener('message', messageListener);
};

})(this, this.document);";

                // Dispose timer no matter if the js call works
                injectJavascriptTimer.Dispose();

#if DEBUG
                Console.WriteLine("Inject JS begin...");
#endif
                // Call the eval with the magic string
                var result = await jsRuntimeService.InvokeAsync<object>("eval", magicString);
#if DEBUG
                Console.WriteLine("Inject JS ready...");
#endif
                // If all goes well call the validate session method
                await ValidateSession();
#if DEBUG
                Console.WriteLine("Validate session called...");
#endif

            }
            else
            {
                injectJavascriptTimer.Start();
            }

        }

    }
}