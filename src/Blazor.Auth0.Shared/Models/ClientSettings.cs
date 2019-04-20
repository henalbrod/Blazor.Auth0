using Blazor.Auth0.Shared.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Shared.Models
{
    public class ClientSettings
    {
        /// <summary>
        /// (String) Your Auth0 account domain (ex. myaccount.auth0.com)
        /// </summary>
        public string Auth0Domain { get; set; }
        /// <summary>
        /// (String) Your Auth0 client ID
        /// </summary>
        public string Auth0ClientId { get; set; }
        /// <summary>
        /// (String) The default scope(s) used by the application. Using scopes can allow you to return specific claims for specific fields in your request. You should read our [documentation on scopes][https://auth0.com/docs/scopes] for further details, default = openid profile email.
        /// </summary>
        public string Auth0Scope { get; set; } = "openid profile email";
        /// <summary>
        /// (String) The default redirectUri used. Defaults to an empty string (none).
        /// </summary>
        public string Auth0RedirectUri { get; set; }
        /// <summary>
        /// (String) Specifies the connection to use rather than presenting all connections available to the application.
        /// </summary>
        public string Auth0Connection { get; set; }
        /// <summary>
        /// (String) The default audience to be used for requesting API access.
        /// </summary>
        public string Auth0Audience { get; set; }
        /// <summary>
        /// (String) The Authentication Granrt Flow to be used (authorization_code recommended).
        /// </summary>
        public AuthenticationGrantTypes AuthenticationGrant { get; set; } = AuthenticationGrantTypes.authorization_code;
        /// <summary>
        /// When set to true, forces a redirection to the login page in case the user is not authenticated
        /// </summary>
        public bool LoginRequired { get; set; }
        /// <summary>
        /// When set to true, forces the redirect_uri param to be the home path, this value overrides Auth0RedirectUri 
        /// </summary>
        public bool RedirectAlwaysToHome { get; set; }
        /// <summary>
        /// When set to true, the serivce will use the id_token payload to build the user info, this is good in case all the user info you require is present in the id_token payload and you want avoid doing an extra call to Auth0, in case that tere's no id_token present in the authentication response the service will fall back gracefully to try to get the user info from an API call to auth0, default = false
        /// </summary>
        public bool GetUserInfoFromIdToken { get; set; } = false;
    }
}
