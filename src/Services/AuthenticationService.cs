using Blazor.Auth0.Models;
using Blazor.Auth0.Models.Enumerations;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Services;
using System.Text;
using System.Timers;

namespace Blazor.Auth0.Authentication
{
    public class AuthenticationService
    {
        public EventHandler<SessionStates> OnSessionStateChanged { get; set; }
        public UserInfoDto User { get; private set; }
        public SessionStates SessionState { get; private set; }

        private readonly ClientSettings clientSettings;
        private readonly IUriHelper uriHelperService;
        private readonly HttpClient httpClientService;
        private readonly IJSInProcessRuntime jsInProcessRuntimeService;
        private string latestAuthorizeUrlCodeChallenge;
        private string latestAuthorizeUrlState;
        private string latestAuthorizeUrRedirectUri;
        private TokenInfo currentSessionTokenInfo;
        private Timer nextSilentLoginTimer;

        public AuthenticationService(HttpClient httpClient, IJSRuntime jsRuntime, IUriHelper uriHelper, ClientSettings settings)
        {
            httpClientService = httpClient;
            jsInProcessRuntimeService = (jsRuntime as IJSInProcessRuntime);
            uriHelperService = uriHelper;
            clientSettings = settings;

            ValidateSession();            

        }

        public string BuildAuthorizeUrl()
        {

            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());

            latestAuthorizeUrlCodeChallenge = GenerateNonce();
            latestAuthorizeUrlState = GenerateNonce();

            if (clientSettings.RedirectAlwaysToHome)
            {
                latestAuthorizeUrRedirectUri = abosulteUri.GetLeftPart(UriPartial.Authority);
            }
            if(!clientSettings.RedirectAlwaysToHome) {
                latestAuthorizeUrRedirectUri = !string.IsNullOrEmpty(clientSettings.Auth0RedirectUri) ?  clientSettings.Auth0RedirectUri : abosulteUri.GetLeftPart(UriPartial.Path);
            }

            return $"https://{clientSettings.Auth0Domain}/authorize?" +
                      "&response_type=code" +
                      "&code_challenge_method=S256" +
                      $"code_challenge={latestAuthorizeUrlCodeChallenge}" +
                      $"&state={latestAuthorizeUrlState}" +
                      $"&client_id={clientSettings.Auth0ClientId}" +
                      $"&scope={clientSettings.Auth0Scope.Replace(" ", "%20")}" +
                      (!string.IsNullOrEmpty(clientSettings.Auth0Connection) ? "&connection=" + clientSettings.Auth0Connection : "") +
                      (!string.IsNullOrEmpty(clientSettings.Auth0Audience) ? "&audience=" + clientSettings.Auth0Audience : "") +
                      $"&redirect_uri={latestAuthorizeUrRedirectUri}";

        }

        public string BuildLogoutUrl() {

            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var host = abosulteUri.GetLeftPart(UriPartial.Authority);
            SetIsLoggedIn(false);
            return $"https://{clientSettings.Auth0Domain}/v2/logout?" +
                   $"client_id={clientSettings.Auth0ClientId}" +
                   $"&returnTo={(clientSettings.RedirectAlwaysToHome ? host : string.IsNullOrEmpty(clientSettings.Auth0RedirectUri) ? host : clientSettings.Auth0RedirectUri )}";

        }
        public void Authorize()
        {
            uriHelperService.NavigateTo(BuildAuthorizeUrl());
        }
        public void LogOut()
        {
            uriHelperService.NavigateTo(BuildLogoutUrl());
        }
        public void ValidateSession()
        {

            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var isLoggedIn = jsInProcessRuntimeService.Invoke<bool>("isLoggedIn", null);
            if (!isLoggedIn && !abosulteUri.Query.Contains("code"))
            {
                if (clientSettings.LoginRequired)
                {
                    uriHelperService.NavigateTo(BuildAuthorizeUrl());
                }
                else
                {
                    SessionState = SessionStates.Inactive;
                    InvokeOnSessionStateChanged();
                }
            }
            else
            {
                jsInProcessRuntimeService.Invoke<object>("drawAuth0Iframe", new DotNetObjectRef(this), $"{BuildAuthorizeUrl()}&response_mode=web_message&prompt=none");
            }

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

                var claims = Json.Deserialize<IDictionary<string, object>>(responseText);

                return GetUserInfoFromClaims(claims);

            }

            return null;

        }
       
        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        [JSInvokable]
        public async Task HandleAuth0Message(Auth0IframeMessage message)
        {
            var abosulteUri = new Uri(uriHelperService.GetAbsoluteUri());
            var origin = new Uri(message.Origin);
            string loginError = null;

            // Validate Origin
            if (!message.IsTrusted || origin.Authority != clientSettings.Auth0Domain) loginError = "Invalid Origin";

            // Validate Error
            if (loginError == null && !string.IsNullOrEmpty(message.Error))
            {

                switch (message.Error.ToLower())
                {
                    case "login_required":

                        loginError = "Login Required";

                        if (clientSettings.LoginRequired)
                        {
                            SetIsLoggedIn(false);
                            uriHelperService.NavigateTo(BuildAuthorizeUrl());
                        }

                        break;
                    default:
                        loginError = message.ErrorDescription;
                        break;
                }

            }

            // Validate State
            if (loginError == null && !string.IsNullOrEmpty(latestAuthorizeUrlState) ? latestAuthorizeUrlState != message.State.Replace(' ', '+') : false) loginError = "Invalid State";


            if (loginError == null)
            {

                await GetAccessToken(message.Code);

                ScheduleSilentLogin();

                if (clientSettings.GetUserInfoFromIdToken && string.IsNullOrEmpty(currentSessionTokenInfo.id_token))
                {
                    // Decode JWT payload into user info
                    User = DecodeTokenPayload(currentSessionTokenInfo.id_token);
                }
                else {
                    // In case we're not getting the id_token from the message response or GetUserInfoFromIdToken is set to false try to get it from Auth0's API
                    User = await UserInfo(currentSessionTokenInfo.access_token);
                }                

                SessionState = SessionStates.Active;

                SetIsLoggedIn(true);

                InvokeOnSessionStateChanged();

            }
            else
            {
                SessionState = SessionStates.Inactive;
                User = null;
                currentSessionTokenInfo = null;
                latestAuthorizeUrlCodeChallenge = null;
                latestAuthorizeUrlState = null;
                nextSilentLoginTimer?.Stop();
                nextSilentLoginTimer?.Dispose();
                SetIsLoggedIn(false);
                Console.WriteLine("Login Error: " + loginError);
                InvokeOnSessionStateChanged();
            }

            // Redirect to home (removing the hash)
            uriHelperService.NavigateTo(abosulteUri.GetLeftPart(UriPartial.Path));

        }

        private void SetIsLoggedIn(bool value)
        {
            jsInProcessRuntimeService.Invoke<object>("setIsLoggedIn", value);
        }
        private async Task GetAccessToken(string code)
        {

            HttpContent content = new StringContent(Json.Serialize(new
            {
                grant_type = "authorization_code",
                client_id = clientSettings.Auth0ClientId,
                code_verifier = latestAuthorizeUrlCodeChallenge,
                code,
                redirect_uri = latestAuthorizeUrRedirectUri
            }), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClientService.PostAsync($@"https://{clientSettings.Auth0Domain}/oauth/token", content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                currentSessionTokenInfo = Json.Deserialize<TokenInfo>(responseText);
            }

        }
        private UserInfoDto DecodeTokenPayload(string token)
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

            var claims = Json.Deserialize<IDictionary<string, object>>(decodedString);

            return GetUserInfoFromClaims(claims);

        }

        private UserInfoDto GetUserInfoFromClaims(IDictionary<string, object> claims) {

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
                        result.CustomClaims.Add(claim.Key,claim.Value);
                        break;
                }

            }

            return result;

        }

        private void ScheduleSilentLogin() {
            
             nextSilentLoginTimer?.Stop();
            
            if(nextSilentLoginTimer == null)
            {                
                nextSilentLoginTimer = new Timer();
                nextSilentLoginTimer.Elapsed += (Object source, ElapsedEventArgs e) => {
                    ValidateSession();
                };
            }

            nextSilentLoginTimer.Interval = currentSessionTokenInfo.expires_in - 5000;

            nextSilentLoginTimer.Start();

        }

        private void InvokeOnSessionStateChanged() {

            if (OnSessionStateChanged != null) {
                OnSessionStateChanged.Invoke(this, SessionState);
            }
             
        }

        private string GenerateNonce() {

            string result = "";

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                result = Convert.ToBase64String(tokenData);
            }

            return result;

        }

    }
}
