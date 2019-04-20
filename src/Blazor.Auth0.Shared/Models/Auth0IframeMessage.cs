using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Shared.Models
{
    public class Auth0IframeMessage
    {
        public bool IsTrusted { get; set; }
        public string Origin { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }

        // Code Grant (Recommended)
        public string Code { get; set; }

        // Implicit Grant (Legacy)        
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }

    }
}
