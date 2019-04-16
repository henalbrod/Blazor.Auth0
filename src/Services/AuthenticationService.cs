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
using Microsoft.AspNetCore.Components;

namespace Blazor.Auth0.Authentication
{
    public class AuthenticationService
    {

        public EventHandler<SessionStates> OnSessionStateChanged { get; set; }
        public UserInfo UserInfo { get; set; }
        public SessionStates SessionState { get; set; }
        public Auth0Settings Auth0Settings { get; set; }
        /// <summary>
        /// Forces the user to be authenticated, redirecting it to the login page automatically in case no active session is present
        /// </summary>     
        public bool LoginRequired { get; set; }

        private IUriHelper _uriHelper { get; set; }
        private HttpClient _httpClient;
        private IJSInProcessRuntime _jsInProcessRuntime { get; set; }
        private string _codeChallenge { get; set; }
        private string _state { get; set; }
        private string _redirectUri { get; set; }
        private TokenInfoDto _tokenInfo { get; set; }
        private Timer _timer { get; set; }


        public AuthenticationService(HttpClient httpClient, IJSRuntime jsRuntime, IUriHelper uriHelper)
        {
            _httpClient = httpClient;
            _jsInProcessRuntime = (jsRuntime as IJSInProcessRuntime);
            _uriHelper = uriHelper;
        }

        public string GetAuthorizeUrl()
        {


            var abosulteUri = new Uri(_uriHelper.GetAbsoluteUri());

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                _codeChallenge = Convert.ToBase64String(tokenData);
                rng.GetBytes(tokenData);
                _state = Convert.ToBase64String(tokenData);
            }

            _redirectUri = !string.IsNullOrEmpty(Auth0Settings.RedirectUri) ? Auth0Settings.RedirectUri : abosulteUri.GetLeftPart(UriPartial.Path);

            var url = $"https://{Auth0Settings.Domain}/authorize?" +
                      "&response_type=code" +
                      "&code_challenge_method=S256" +
                      $"code_challenge={_codeChallenge}" +
                      $"&state={_state}" +
                      $"&client_id={Auth0Settings.ClientId}" +
                      $"&scope={Auth0Settings.Scope.Replace(" ", "%20")}" +
                      (!string.IsNullOrEmpty(Auth0Settings.Connection) ? "&connection=" + Auth0Settings.Connection : "") +
                      (!string.IsNullOrEmpty(Auth0Settings.Audience) ? "&audience=" + Auth0Settings.Audience : "") +
                      $"&redirect_uri={_redirectUri}";

            return url;

        }
        public void LogIn()
        {
            _uriHelper.NavigateTo(GetAuthorizeUrl());
        }
        public void LogOut()
        {
            var abosulteUri = new Uri(_uriHelper.GetAbsoluteUri());
            SetIsLoggedIn(false);
            _uriHelper.NavigateTo($"https://{Auth0Settings.Domain}/v2/logout?" +
                                  $"client_id={Auth0Settings.ClientId}" +
                                  $"&returnTo={(!string.IsNullOrEmpty(Auth0Settings.RedirectUri) ? Auth0Settings.RedirectUri : abosulteUri.GetLeftPart(UriPartial.Authority))}");
        }
        public void ValidateSession()
        {

            var abosulteUri = new Uri(_uriHelper.GetAbsoluteUri());
            var isLoggedIn = _jsInProcessRuntime.Invoke<bool>("isLoggedIn", null);
            if (!isLoggedIn && !abosulteUri.Query.Contains("code"))
            {
                if (LoginRequired)
                {
                    _uriHelper.NavigateTo(GetAuthorizeUrl());
                }
                else
                {
                    SessionState = SessionStates.Inactive;
                    InvokeOnSessionStateChanged();
                }
            }
            else
            {
                _jsInProcessRuntime.Invoke<object>("drawAuth0Iframe", new DotNetObjectRef(this), $"{GetAuthorizeUrl()}&response_mode=web_message&prompt=none");
            }

        }
        public async Task<UserInfo> GetUserInfo(string accessToken)
        {

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await _httpClient.GetAsync($@"https://{Auth0Settings.Domain}/userinfo");

            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new UserInfo(Json.Deserialize<Dictionary<string, object>>(responseText));
            }

            return null;

        }
        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        [JSInvokable]
        public async Task HandleAuth0Message(Auth0IframeMessage message)
        {
            var abosulteUri = new Uri(_uriHelper.GetAbsoluteUri());
            var origin = new Uri(message.Origin);
            string loginError = null;

            // Validate Origin
            if (!message.IsTrusted || origin.Authority != Auth0Settings.Domain) loginError = "Invalid Origin";

            // Validate Error
            if (loginError == null && !string.IsNullOrEmpty(message.Error))
            {

                switch (message.Error.ToLower())
                {
                    case "login_required":

                        loginError = "Login Required";

                        if (LoginRequired)
                        {
                            SetIsLoggedIn(false);
                            _uriHelper.NavigateTo(GetAuthorizeUrl());
                        }

                        break;
                    default:
                        loginError = message.ErrorDescription;
                        break;
                }

            }

            // Validate State
            if (loginError == null && !string.IsNullOrEmpty(_state) ? _state != message.State.Replace(' ', '+') : false) loginError = "Invalid State";


            if (loginError == null)
            {

                await GetAccessToken(message.Code);

                ScheduleSilentLogin();

                // In case we're not getting the id_token from the message try to get it from Auth0's API
                if (string.IsNullOrEmpty(_tokenInfo.id_token))
                {
                    UserInfo = await GetUserInfo(_tokenInfo.access_token);
                }
                else
                {
                    // Decode JWT payload into user info
                    UserInfo = DecodeTokenPayload(_tokenInfo.id_token);
                }

                SessionState = SessionStates.Active;

                SetIsLoggedIn(true);

                InvokeOnSessionStateChanged();

            }
            else
            {
                SessionState = SessionStates.Inactive;
                UserInfo = null;
                _tokenInfo = null;
                _codeChallenge = null;
                _state = null;
                _timer?.Stop();
                _timer?.Dispose();
                SetIsLoggedIn(false);
                Console.WriteLine("Login Error: " + loginError);
                InvokeOnSessionStateChanged();
            }

            // Redirect to home (removing the hash)
            _uriHelper.NavigateTo(abosulteUri.GetLeftPart(UriPartial.Path));

        }

        private void SetIsLoggedIn(bool value)
        {
            _jsInProcessRuntime.Invoke<object>("setIsLoggedIn", value);
        }
        private async Task GetAccessToken(string code)
        {

            HttpContent content = new StringContent(Json.Serialize(new
            {
                grant_type = "authorization_code",
                client_id = Auth0Settings.ClientId,
                code_verifier = _codeChallenge,
                code = code,
                redirect_uri = _redirectUri
            }), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($@"https://{Auth0Settings.Domain}/oauth/token", content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _tokenInfo = Json.Deserialize<TokenInfoDto>(responseText);
            }

        }
        private UserInfo DecodeTokenPayload(string token)
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

            var claimsInfo = Json.Deserialize<Dictionary<string, object>>(decodedString);

            return new UserInfo(claimsInfo);

        }
        private void ScheduleSilentLogin() {
            
             _timer?.Stop();
            
            if(_timer == null)
            {
                Console.WriteLine("Creating timer");
                _timer = new Timer();
                _timer.Elapsed += (Object source, ElapsedEventArgs e) => {
                    Console.WriteLine("Calling silent login");
                    ValidateSession();
                };
            }

            _timer.Interval = _tokenInfo.expires_in - 5000;

            _timer.Start();

            Console.WriteLine("Scheduling Silent Login into " + _timer.Interval + " miliseconds");

        }

        private void InvokeOnSessionStateChanged() {

            if (OnSessionStateChanged != null) {
                OnSessionStateChanged.Invoke(this, SessionState);
            }
             
        }

    }
}
