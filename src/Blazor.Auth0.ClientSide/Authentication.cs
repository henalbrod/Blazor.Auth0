// <copyright file="Authentication.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
    using Blazor.Auth0.ClientSide.Properties;
    using Blazor.Auth0.Models;
    using Blazor.Auth0.Models.Enumerations;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Http;
    using Microsoft.JSInterop;

    /// <summary>
    /// Authentication flow methods.
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Builds the an authorization URI.
        /// </summary>
        /// <param name="buildAuthorizedUrlOptions">A <see cref="AuthorizeOptions"/> param.</param>
        /// <returns>An <see cref="string"/> representing an authorization URI.</returns>
        public static string BuildAuthorizeUrl(AuthorizeOptions buildAuthorizedUrlOptions)
        {
            if (buildAuthorizedUrlOptions is null)
            {
                throw new ArgumentNullException(nameof(buildAuthorizedUrlOptions));
            }

            Utils.ValidateObject(buildAuthorizedUrlOptions);

            var responseType = CommonAuthentication.ParseResponseType(buildAuthorizedUrlOptions.ResponseType);
            var responseMode = CommonAuthentication.ParseResponseMode(buildAuthorizedUrlOptions.ResponseMode);

            var path = new PathString("/authorize");
            var query = new QueryString();

            query = query.Add("response_type", responseType);
            query = query.Add("state", buildAuthorizedUrlOptions.State);
            query = query.Add("nonce", buildAuthorizedUrlOptions.Nonce);
            query = query.Add("client_id", buildAuthorizedUrlOptions.ClientID);
            query = query.Add("scope", buildAuthorizedUrlOptions.Scope);            

            if (buildAuthorizedUrlOptions.CodeChallengeMethod != CodeChallengeMethods.None)
            {
                string codechallengeMethod = CommonAuthentication.ParseCodeChallengeMethod(buildAuthorizedUrlOptions.CodeChallengeMethod);

                query = query.Add("code_challenge_method", codechallengeMethod);
                query = query.Add("code_challenge", buildAuthorizedUrlOptions.CodeChallenge);
            }

            if (!string.IsNullOrEmpty(buildAuthorizedUrlOptions.Connection))
            {
                query = query.Add("connection", buildAuthorizedUrlOptions.Connection);
            }

            if (!string.IsNullOrEmpty(buildAuthorizedUrlOptions.Audience))
            {
                query = query.Add("audience", buildAuthorizedUrlOptions.Audience);
            }

            if (!string.IsNullOrEmpty(responseMode))
            {
                query = query.Add("response_mode", responseMode);
            }

            query = query.Add("redirect_uri", buildAuthorizedUrlOptions.RedirectUri);

            UriBuilder uriBuilder = new UriBuilder()
            {
                Scheme = "https",
                Host = buildAuthorizedUrlOptions.Domain,
                Path = path,
                Query = query.ToUriComponent(),
            };

            return uriBuilder.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Initiates the Authorization flow by calling the IDP's /authorize enpoint.
        /// </summary>
        /// <param name="jsRuntime">A <see cref="IJSRuntime"/> param.</param>
        /// <param name="uriHelper">A <see cref="IUriHelper"/> param.</param>
        /// <param name="authorizeOptions">A <see cref="AuthorizeOptions"/> param.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task Authorize(IJSRuntime jsRuntime, NavigationManager uriHelper, AuthorizeOptions authorizeOptions)
        {
            if (jsRuntime is null)
            {
                throw new ArgumentNullException(nameof(jsRuntime));
            }

            if (uriHelper is null)
            {
                throw new ArgumentNullException(nameof(uriHelper));
            }

            if (authorizeOptions is null)
            {
                throw new ArgumentNullException(nameof(authorizeOptions));
            }

            Utils.ValidateObject(authorizeOptions);

            authorizeOptions = await TransactionManager.Proccess(jsRuntime, authorizeOptions).ConfigureAwait(false);

            string authorizeUrl = BuildAuthorizeUrl(authorizeOptions);

            uriHelper.NavigateTo(authorizeUrl);
        }

        /// <summary>
        /// Get an Access Token in order to call an API.
        /// </summary>
        /// <param name="httpClient">A <see cref="HttpClient"/> param.</param>
        /// <param name="auth0domain">The Auth0's tenant domain.</param>
        /// <param name="auth0ClientId">The Auth0's client id.</param>
        /// <param name="code">The code received from Auth0.</param>
        /// <param name="audience">The Auth0's audience domain.</param>
        /// <param name="codeVerifier">The code verification token used in the authentication flow</param>
        /// <param name="redirectUri">URL to redirect the user after the logout.</param>
        /// <param name="secret">The Auth0's client secret</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<SessionInfo> GetAccessToken(
            HttpClient httpClient,
            string auth0domain,
            string auth0ClientId,
            string code,
            string audience = null,
            string codeVerifier = null,
            string redirectUri = null,
            string secret = null)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(auth0domain))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(auth0domain));
            }

            if (string.IsNullOrEmpty(auth0ClientId))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(auth0ClientId));
            }

            if (string.IsNullOrEmpty(codeVerifier) && string.IsNullOrEmpty(secret))
            {
                throw new ArgumentException(Resources.MissingPKCERequiredParamError, $"{nameof(secret)} or {nameof(codeVerifier)}");
            }

            if (!string.IsNullOrEmpty(codeVerifier) && !string.IsNullOrEmpty(secret))
            {
                throw new ArgumentException(Resources.DuplicatedPKCERequiredParamError, $"{nameof(secret)} and {nameof(codeVerifier)}");
            }

            SessionInfo response = null;

            using (HttpContent content = new StringContent(
                JsonSerializer.Serialize(
                    new
                    {
                        grant_type = "authorization_code",
                        client_id = auth0ClientId,
                        audience,
                        code,
                        code_verifier = codeVerifier,
                        redirect_uri = redirectUri,
                        client_secret = secret,
                    },
                    new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                    }), Encoding.UTF8,
                        Resources.ApplicationJsonMediaType
                    )
                )
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($@"https://{auth0domain}/oauth/token", content).ConfigureAwait(false);

                string responseText = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    response = JsonSerializer.Deserialize<SessionInfo>(responseText);
                }
            }

            return response;
        }

        /// <summary>
        /// Parses the fragment of the <see cref="Uri"/> received after the user's authentication flow, into a <see cref="ParsedHash"/>.
        /// </summary>
        /// <param name="parseHashOptions">The <see cref="ParseHashOptions"/>.</param>
        /// <returns>A <see cref="ParsedHash"/>.</returns>
        public static ParsedHash ParseHash(ParseHashOptions parseHashOptions)
        {
            if (parseHashOptions is null)
            {
                throw new ArgumentNullException(nameof(parseHashOptions));
            }

            Utils.ValidateObject(parseHashOptions);

            Uri absoluteUri = parseHashOptions.AbsoluteUri;
            string hash;
            switch (parseHashOptions.ResponseType)
            {
                case ResponseTypes.Code:
                hash = !string.IsNullOrEmpty(absoluteUri.Query) ? absoluteUri.Query.Remove(0, 1) : null;
                break;
                default:
                {
                    string[] fragments = absoluteUri.AbsoluteUri.Split('#');

                    if (fragments.Length < 2)
                    {
                        return null;
                    }

                    hash = fragments[1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(hash))
            {
                return null;
            }

            // TODO: Improve this as it can fail :S
            string hashJson = "{\"" + string.Join("\", \"", hash.Split('&')).Replace("=", "\":\"") + "\"}";
            ParsedHash result = JsonSerializer.Deserialize<ParsedHash>(hashJson);

            if (!string.IsNullOrEmpty(result.Error))
            {
                string error = $"{result.Error}: {result.ErrorDescription}";

                error += !string.IsNullOrEmpty(result.State) ? $"; state: {result.State}" : string.Empty;

                throw new Exception(error);
            }

            if (
               string.IsNullOrEmpty(result.Code) &&
               string.IsNullOrEmpty(result.AccessToken) &&
               string.IsNullOrEmpty(result.IdToken) &&
               string.IsNullOrEmpty(result.RefreshToken))
            {
                return null;
            }

            if ((parseHashOptions.ResponseType == ResponseTypes.Token || parseHashOptions.ResponseType == ResponseTypes.TokenAndIdToken) && string.IsNullOrEmpty(result.AccessToken))
            {
                throw new Exception(Resources.InvalidHashMissingAccessTokenError);
            }

            if ((parseHashOptions.ResponseType == ResponseTypes.Token || parseHashOptions.ResponseType == ResponseTypes.TokenAndIdToken) && string.IsNullOrEmpty(result.IdToken))
            {
                throw new Exception(Resources.InvalidHashMissingIdTokenError);
            }

            return result;
        }

        /// <summary>
        /// Validates an <see cref="AuthenticationResponse"/>.
        /// </summary>
        /// <param name="authorizationResponse">The <see cref="AuthenticationResponse"/> pararm.</param>
        /// <param name="auth0Domain">The Auth0's tenant domain.</param>
        /// <param name="state">The state used during the authentication flow.</param>
        public static void ValidateAuthorizationResponse(AuthorizationResponse authorizationResponse, string auth0Domain, string state)
        {
            if (string.IsNullOrEmpty(auth0Domain))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(auth0Domain));
            }

            if (authorizationResponse is null)
            {
                throw new ArgumentNullException(nameof(authorizationResponse));
            }

            string errorDescription = null;
            Uri origin = new Uri(authorizationResponse.Origin);

            // Validate Origin
            if (authorizationResponse.IsTrusted && origin.Authority != auth0Domain)
            {
                errorDescription = "Invalid Origin";
            }

            // Validate Error
            if (errorDescription == null && !string.IsNullOrEmpty(authorizationResponse.Error))
            {
                switch (authorizationResponse.Error.ToLower())
                {
                    case "login_required":

                    errorDescription = "Login Required";

                    break;
                    default:
                    errorDescription = authorizationResponse.ErrorDescription;
                    break;
                }
            }

            // Validate State
            if (errorDescription == null && !string.IsNullOrEmpty(state) ? state != authorizationResponse.State.Replace(' ', '+') : false)
            {
                errorDescription = "Invalid State";
            }

            if (!string.IsNullOrEmpty(errorDescription))
            {
                throw new ApplicationException(errorDescription);
            }
        }

        /// <summary>
        /// Validates the at_hash claim of an access token.
        /// </summary>
        /// <param name="atHash">The at_hash string.</param>
        /// <param name="accessToken">The access_token string.</param>
        /// <returns>A <see cref="bool"/> representing the validation result.</returns>
        public static bool ValidateAccessTokenHash(string atHash, string accessToken)
        {
            if (!string.IsNullOrEmpty(atHash))
            {
                string accessTokenHash = string.Empty;

                using (SHA256 mySHA256 = SHA256.Create())
                {
                    byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                    string base64Encoded = Convert.ToBase64String(hashValue.Take(16).ToArray());
                    accessTokenHash = Convert.ToBase64String(hashValue.Take(16).ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
                }

                return accessTokenHash.Equals(atHash);
            }

            return false;
        }
    }
}
