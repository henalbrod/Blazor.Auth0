// <copyright file="ClientOptions.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Blazor.Auth0.Models.Enumerations;

    /// <summary>
    /// Class for handling the Auth0's client options required in the authentication flow.
    /// </summary>
    public class ClientOptions : IValidatableObject
    {
        public string ClaimsIssuer { get; set; } = "Auth0";

        /// <summary>
        /// Gets or sets your Auth0 account domain (ex. myaccount.auth0.com).
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets your Auth0 client ID.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets url that the Auth0 will redirect to after user authentication. Defaults to an empty string (none). This value overrides RedirectAlwaysToHome.
        /// </summary>
        public string RedirectUri { get; set; }

        public string AuthorizePath { get; set; } = "/account/authorize";

        public string CallbackPath { get; set; } = "/account/callback";

        public string RemoteSignOutPath { get; set; } = "/account/logout";

        /// <summary>
        /// Gets or sets type of the response used by OAuth 2.0 flow. It can be any space separated list of the values `code`, `token`, `id_token` https://openid.net/specs/oauth-v2-multiple-response-types-1_0.html.
        /// </summary>
        public ResponseTypes ResponseType { get; set; } = ResponseTypes.Code;

        /// <summary>
        /// Gets or sets row the Auth response is encoded and redirected back to the client. Supported values are `query`, `fragment` and `form_post`.
        /// </summary>
        public ResponseModes ResponseMode { get; set; } = ResponseModes.Query;

        /// <summary>
        /// Gets or sets the default scope(s) used by the application. Using scopes can allow you to return specific claims for specific fields in your request. You should read our [documentation on scopes][https://auth0.com/docs/scopes] for further details, default = openid profile email.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string Scope { get; set; } = "openid profile email";

        /// <summary>
        /// Gets or sets identifier of the resource server who will consume the access token issued after Auth.
        /// </summary>
        public string Audience { get; set; }

        #region Miscellaneous

        /// <summary>
        /// Gets or sets a value indicating whether when set to true, forces a redirection to the login page in case the user is not authenticated.
        /// </summary>
        public bool RequireAuthenticatedUser { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether when set to true, forces the redirect_uri param to be the home path, this value can be overridden by RedirectUri.
        /// </summary>
        public bool RedirectAlwaysToHome { get; set; } = true;

        public bool SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether when set to true, the serivce will use the id_token payload to build the user info, this is good in case all the user info you require is present in the id_token payload and you want avoid doing an extra call to Auth0, in case that tere's no id_token present in the authentication response the service will fall back gracefully to try to get the user info from an API call to auth0, default = false.
        /// </summary>
        public bool GetUserInfoFromIdToken { get; set; }

        public string Namespace { get; set; } = "com.auth0.auth.";

        public int KeyLength { get; set; } = 32;

        public string Realm { get; set; }

        public LoginModes LoginMode { get; set; } = LoginModes.Redirect;

        #endregion

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>
            {
                ScopeValidation.ScopeValidate(this.Scope),
            };

            return results;
        }
    }
}
