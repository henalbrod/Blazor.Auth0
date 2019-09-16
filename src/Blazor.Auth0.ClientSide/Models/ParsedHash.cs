// <copyright file="ParsedHash.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class representing the fragment part of a <see cref="Uri"/> after the user's authentication flow.
    /// </summary>
    public class ParsedHash
    {

        /// <summary>
        /// Gets or sets the state parameter value of the hash.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the error parameter value of the hash.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error_description parameter value of the hash.
        /// </summary>
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }

        // Code Grant (Recommended)

        /// <summary>
        /// Gets or sets the code parameter value of the hash.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        // Implicit Grant (Legacy)

        /// <summary>
        /// Gets or sets the access_token parameter value of the hash.
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the id_token parameter value of the hash.
        /// </summary>
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh_token parameter value of the hash.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
