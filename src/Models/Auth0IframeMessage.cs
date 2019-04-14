using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Models
{
    public class Auth0IframeMessage
    {
        public bool IsTrusted { get; set; }
        public string Origin { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
    }
}
