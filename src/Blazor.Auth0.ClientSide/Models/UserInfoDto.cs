using Blazor.Auth0.ClientSide.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.ClientSide.Models
{
    public class UserInfoDto
    {

        public string Address { get; internal set; }
        public string Birthdate { get; internal set; }
        public string Email { get; internal set; }
        public bool EmailVerified { get; internal set; }
        public string FamilyName { get; internal set; }
        public string Gender { get; internal set; }
        public string GivenName { get; internal set; }
        public string Locale { get; internal set; }
        public string MiddleName { get; internal set; }
        public string Name { get; internal set; }
        public string Nickname { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public bool PhoneNumberVerified { get; internal set; }
        public string Picture { get; internal set; }
        public string PreferredUsername { get; internal set; }
        public string Profile { get; internal set; }
        public string Sub { get; internal set; }
        public DateTime UpdatedAt { get; internal set; }
        public string Website { get; internal set; }
        public string Zoneinfo { get; internal set; }
        public Dictionary<string, object> CustomClaims { get; internal set; } = new Dictionary<string, object>();

    }
}
