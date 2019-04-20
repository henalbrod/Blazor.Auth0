using Blazor.Auth0.Shared.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Shared.Models
{
    public class UserInfoDto
    {

        public string Address { get; set; }
        public string Birthdate { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string FamilyName { get; set; }
        public string Gender { get; set; }
        public string GivenName { get; set; }
        public string Locale { get; set; }
        public string MiddleName { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberVerified { get; set; }
        public string Picture { get; set; }
        public string PreferredUsername { get; set; }
        public string Profile { get; set; }
        public string Sub { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Website { get; set; }
        public string Zoneinfo { get; set; }
        public Dictionary<string, object> CustomClaims { get; set; } = new Dictionary<string, object>();

    }
}
