// <copyright file="CommonAuthentication.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Blazor.Auth0.Models;
    using Blazor.Auth0.Models.Enumerations;
    using Blazor.Auth0.Shared.Properties;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Common authentication flow methods.
    /// </summary>
    public static class CommonAuthentication
    {
        /// <summary>
        /// Builds a log out URI.
        /// </summary>
        /// <param name="auth0Domain">The Auth0's tenant domain.</param>
        /// <returns>A <see cref="string"/> representing the log out url.</returns>
        public static string BuildLogoutUrl(string auth0Domain)
        {
            return BuildLogoutUrl(auth0Domain, null, null);
        }

        /// <summary>
        /// Builds a log out URI.
        /// </summary>
        /// <param name="auth0Domain">The Auth0's tenant domain.</param>
        /// <param name="auth0ClientId">The Auth0's client id.</param>
        /// <returns>A <see cref="string"/> representing the log out url.</returns>
        public static string BuildLogoutUrl(string auth0Domain, string auth0ClientId)
        {
            return BuildLogoutUrl(auth0Domain, auth0ClientId, null);
        }

        /// <summary>
        /// Builds a log out URI.
        /// </summary>
        /// <param name="auth0Domain">The Auth0's tenant domain.</param>
        /// <param name="auth0ClientId">The Auth0's client id.</param>
        /// <param name="redirectUri">The URI to redirect the user after the logout.</param>
        /// <returns>A <see cref="string"/> representing the log out url.</returns>
        public static string BuildLogoutUrl(string auth0Domain, string auth0ClientId, string redirectUri)
        {
            if (string.IsNullOrEmpty(auth0Domain))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(auth0Domain));
            }

            var path = new PathString("/v2/logout");
            var query = new QueryString();

            if (!string.IsNullOrEmpty(auth0ClientId))
            {
                query = query.Add("client_id", auth0ClientId);
            }

            if (!string.IsNullOrEmpty(redirectUri))
            {
                query = query.Add("returnTo", redirectUri);
            }

            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = auth0Domain,
                Path = path,
                Query = query.ToUriComponent(),
            };

            // TODO: Implement propper Uri creation
            return uri.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Makes a call to the /userinfo endpoint and returns the user profile.
        /// </summary>
        /// <param name="httpClient">A <see cref="HttpClient"/> instance.</param>
        /// <param name="auth0Domain">The Auth0's tenant domain.</param>
        /// <param name="accessToken">The access_token received after the user authentication flow.</param>
        /// <returns>A <see cref="UserInfo"/>.</returns>
        public static async Task<UserInfo> UserInfo(HttpClient httpClient, string auth0Domain, string accessToken)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(auth0Domain))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(auth0Domain));
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(accessToken));
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.GetAsync($@"https://{auth0Domain}/userinfo").ConfigureAwait(false);

            string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<UserInfo>(responseText);
            }

            return null;
        }

        /// <summary>
        /// Decodes a JWT payload into the indicated type.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="token">The JWT.</param>
        /// <returns>An instance of the indicated type.</returns>
        public static T DecodeTokenPayload<T>(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(Resources.NullArgumentExceptionError, nameof(token));
            }

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

            byte[] bytesArray = Convert.FromBase64String(tokenPayload);
            string decodedString = Encoding.UTF8.GetString(bytesArray, 0, bytesArray.Count());

            T result = JsonSerializer.Deserialize<T>(decodedString);

            return result;
        }

        /// <summary>
        /// Gets a nonce string of the indicated key lenght.
        /// </summary>
        /// <param name="keyLenght">The length of the key.</param>
        /// <returns>A <see cref="string"/> representing the nonce.</returns>
        public static string GenerateNonce(int keyLenght)
        {
            string result = string.Empty;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[keyLenght];
                rng.GetBytes(tokenData);
                result = Convert.ToBase64String(tokenData).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }

            return result;
        }

        /// <summary>
        /// Gets the string representation of a <see cref="ResponseTypes"/>.
        /// </summary>
        /// <param name="responseType">The <see cref="ResponseTypes"/>.</param>
        /// <returns>A <see cref="string"/> representing the <see cref="ResponseTypes"/>.</returns>
        public static string ParseResponseType(ResponseTypes responseType)
        {
            string result;

            switch (responseType)
            {
                case ResponseTypes.TokenAndIdToken:
                result = "token id_token";
                break;
                case ResponseTypes.IdToken:
                result = "id_token";
                break;
                case ResponseTypes.Token:
                result = "token";
                break;
                default:
                result = "code";
                break;
            }

            return result;
        }

        /// <summary>
        /// Gets the string respresentation of a <see cref="CodeChallengeMethods"/>.
        /// </summary>
        /// <param name="codeChallengeMethod">The <see cref="CodeChallengeMethods"/>.</param>
        /// <returns>A <see cref="string"/> representing the <see cref="CodeChallengeMethods"/>.</returns>
        public static string ParseCodeChallengeMethod(CodeChallengeMethods codeChallengeMethod)
        {
            string result = string.Empty;

            if (codeChallengeMethod == CodeChallengeMethods.S256)
            {
                result = "S256";
            }

            return result;
        }

        /// <summary>
        /// Gets the string respresentation of a <see cref="ResponseModes"/>.
        /// </summary>
        /// <param name="codeChallengeMethod">The <see cref="ResponseModes"/>.</param>
        /// <returns>A <see cref="string"/> representing the <see cref="ResponseModes"/>.</returns>
        public static string ParseResponseMode(ResponseModes responseMode)
        {
            string result = string.Empty;

            switch (responseMode)
            {
                case ResponseModes.Form_Post:
                result = "form_post";
                break;
                case ResponseModes.Fragment:
                result = "fragment";
                break;
                case ResponseModes.Query:
                result = "query";
                break;
                case ResponseModes.Web_Message:
                result = "web_message";
                break;
            }

            return result;
        }
    }
}
