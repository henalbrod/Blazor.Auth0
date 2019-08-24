// <copyright file="AuthorizationResponse.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class representing the response of an authentication flow.
    /// </summary>
    public class AuthorizationResponse : SessionInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or set if the response is comming from a trusted origin.
        /// </summary>
        public bool IsTrusted { get; set; }

        /// <summary>
        /// Gets or sets the origin of the response.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the tupe of the response.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the state indicated in the authentication flow.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the error code of the authentication flow.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the error description of the authentication flow.
        /// </summary>
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the code indicated in the authentication flow.
        /// </summary>
        public string Code { get; set; }

    }
}
