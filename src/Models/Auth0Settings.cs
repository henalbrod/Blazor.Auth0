using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Models
{
    public class Auth0Settings
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string Scope { get; set; } = "openid profile";
        public string RedirectUri { get; set; }
        public string Connection { get; set; }
        public string Audience { get; set; }
    }
}
