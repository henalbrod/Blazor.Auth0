namespace Blazor.Auth0.Shared.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class for representing the openid configuration.
    /// </summary>
    public class OpenidConfiguration
    {
        /// <summary>
        /// Gets or sets issuer.
        /// </summary>
        [JsonPropertyName("issuer")]
        public Uri Issuer { get; set; }

        /// <summary>
        /// Gets or sets jwks_uri.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public Uri JwksUri { get; set; }

        /// <summary>
        /// Gets or sets authorization endpoint.
        /// </summary>
        [JsonPropertyName("authorization_endpoint")]
        public Uri AuthorizationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets token endpoint.
        /// </summary>
        [JsonPropertyName("token_endpoint")]
        public Uri TokenEndpoint { get; set; }

        /// <summary>
        /// Gets or sets userinfo endpoint.
        /// </summary>
        [JsonPropertyName("userinfo_endpoint")]
        public Uri UserinfoEndpoint { get; set; }

        /// <summary>
        /// Gets or sets end session endpoint.
        /// </summary>
        [JsonPropertyName("end_session_endpoint")]
        public Uri EndSessionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets issuer check session iframe.
        /// </summary>
        [JsonPropertyName("check_session_iframe")]
        public Uri CheckSessionIframe { get; set; }

        /// <summary>
        /// Gets or sets revocation endpoint.
        /// </summary>
        [JsonPropertyName("revocation_endpoint")]
        public Uri RevocationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets introspection endpoint.
        /// </summary>
        [JsonPropertyName("introspection_endpoint")]
        public Uri IntrospectionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets device authorization endpoint.
        /// </summary>
        [JsonPropertyName("device_authorization_endpoint")]
        public Uri DeviceAuthorizationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether frontchannel logout supported.
        /// </summary>
        [JsonPropertyName("frontchannel_logout_supported")]
        public bool FrontchannelLogoutSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether frontchannel logout session supported.
        /// </summary>
        [JsonPropertyName("frontchannel_logout_session_supported")]
        public bool FrontchannelLogoutSessionSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether backchannel logout supported.
        /// </summary>
        [JsonPropertyName("backchannel_logout_supported")]
        public bool BackchannelLogoutSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether backchannel logout session supported.
        /// </summary>
        [JsonPropertyName("backchannel_logout_session_supported")]
        public bool BackchannelLogoutSessionSupported { get; set; }

        /// <summary>
        /// Gets or sets scopes supported.
        /// </summary>
        [JsonPropertyName("scopes_supported")]
        public string[] ScopesSupported { get; set; }

        /// <summary>
        /// Gets or sets claims supported.
        /// </summary>
        [JsonPropertyName("claims_supported")]
        public object[] ClaimsSupported { get; set; }

        /// <summary>
        /// Gets or sets grant types supported.
        /// </summary>
        [JsonPropertyName("grant_types_supported")]
        public string[] GrantTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets response types supported.
        /// </summary>
        [JsonPropertyName("response_types_supported")]
        public string[] ResponseTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets response modes supported.
        /// </summary>
        [JsonPropertyName("response_modes_supported")]
        public string[] ResponseModesSupported { get; set; }

        /// <summary>
        /// Gets or sets token endpoint auth methods supported.
        /// </summary>
        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public string[] TokenEndpointAuthMethodsSupported { get; set; }

        /// <summary>
        /// Gets or sets subject types supported.
        /// </summary>
        [JsonPropertyName("subject_types_supported")]
        public string[] SubjectTypesSupported { get; set; }

        /// <summary>
        /// Gets or sets token signing values supported.
        /// </summary>
        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public string[] IdTokenSigningAlgValuesSupported { get; set; }

        /// <summary>
        /// Gets or sets code challenge methods supported.
        /// </summary>
        [JsonPropertyName("code_challenge_methods_supported")]
        public string[] CodeChallengeMethodsSupported { get; set; }
    }
}
