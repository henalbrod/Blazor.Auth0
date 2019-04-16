using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Models
{
    public class ClientSettings
    {
        public string Auth0Domain { get; set; }
        public string Auth0ClientId { get; set; }
        public string Auth0Scope { get; set; } = "openid profile email";
        public string Auth0RedirectUri { get; set; }
        public string Auth0Connection { get; set; }
        public string Auth0Audience { get; set; }
        /// <summary>
        /// Determines if the user is required to be authenticated
        /// </summary>
        public bool LoginRequired { get; set; }
        /// <summary>
        /// When set to true, forces the redirect_uri param to be the home path, this value overrides Auth0RedirectUri 
        /// </summary>
        public bool RedirectAlwaysToHome { get; set; }
    }
}
