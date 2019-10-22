// <copyright file="Authentication.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
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
        /// Gets a new access_token and id_token from exchanging a refresh token.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance.</param>
        /// <param name="auth0domain">The Auth0's tenant domain.</param>
        /// <param name="audience">The Auth0's audience.</param>
        /// <param name="clientId">The Auth0's client id.</param>
        /// <param name="clientSecret">The Auth0's client secret.</param>
        /// <param name="token">The refresh token.</param>
        /// <returns>A <see cref="AuthenticationResponse"/> instance.</returns>
        public static async Task<AuthorizationResponse> RefreshToken(HttpClient httpClient, string auth0domain, string audience, string clientId, string clientSecret, string token)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(auth0domain))
            {
                throw new ArgumentException(nameof(auth0domain));
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new ArgumentException("message", nameof(audience));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException(nameof(clientSecret));
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException(nameof(token));
            }

            AuthorizationResponse response;

            using (HttpContent content = new StringContent(
                JsonSerializer.Serialize(
                    new
                    {
                        grant_type = "refresh_token",
                        client_id = clientId,
                        client_secret = clientSecret,
                        refresh_token = token,
                        audience,
                    },
                    new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                    }), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($@"https://{auth0domain}/oauth/token", content).ConfigureAwait(false);

                string responseText = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                response = JsonSerializer.Deserialize<AuthorizationResponse>(responseText);

            }

            return response;
        }

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance.</param>
        /// <param name="auth0domain">The Auth0's tenant domain.</param>
        /// <param name="audience">The Auth0's audience.</param>
        /// <param name="clientId">The Auth0's client id.</param>
        /// <param name="clientSecret">The Auth0's client secret.</param>
        /// <param name="refreshToken">The Auth0's slient secret.</param>
        /// <returns>A <see cref="string"/> representing the action result.</returns>
        public static async Task<string> RevokeRefreshToken(HttpClient httpClient, string auth0domain, string audience, string clientId, string clientSecret, string refreshToken)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(auth0domain))
            {
                throw new ArgumentException(nameof(auth0domain));
            }

            if (string.IsNullOrEmpty(audience))
            {
                throw new ArgumentException("message", nameof(audience));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException(nameof(clientSecret));
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException(nameof(refreshToken));
            }

            string response;

            using (HttpContent content = new StringContent(
                JsonSerializer.Serialize(
                    new
                    {
                        client_id = clientId,
                        client_secret = clientSecret,
                        token = refreshToken,
                        audience
                    },
                    new JsonSerializerOptions
                    {
                        IgnoreNullValues = true,
                    }), Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($@"https://{auth0domain}/oauth/revoke", content).ConfigureAwait(false);

                response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            }

            return response;

        }

        /// <summary>
        /// Clears the session cookies.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> instance.</param>
        public static void ClearAspNetCookies(HttpContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Response.Cookies.Delete(".AspNetCore.Cookies");
            context.Response.Cookies.Delete(".AspNetCore.CookiesC1");
            context.Response.Cookies.Delete(".AspNetCore.CookiesC2");
        }
    }
}
