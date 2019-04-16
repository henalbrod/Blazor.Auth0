using Blazor.Auth0.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Models
{
    public class UserInfo
    {

        public string Address { get; private set; }
        public string Birthdate { get; private set; }
        public string Email { get; private set; }
        public bool EmailVerified { get; private set; }
        public string FamilyName { get; private set; }
        public string Gender { get; private set; }
        public string GivenName { get; private set; }
        public string Locale { get; private set; }
        public string MiddleName { get; private set; }
        public string Name { get; private set; }
        public string Nickname { get; private set; }
        public string PhoneNumber { get; private set; }
        public bool PhoneNumberVerified { get; private set; }
        public string Picture { get; private set; }
        public string PreferredUsername { get; private set; }
        public string Profile { get; private set; }
        public string Sub { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public string Website { get; private set; }
        public string Zoneinfo { get; private set; }

        internal UserInfo(Dictionary<string, object> claimsInfo)
        {

            var claims = claimsInfo ?? new Dictionary<string, object>();

            foreach (var claim in claims)
            {

                switch (claim.Key)
                {
                    case nameof(StandarClaims.address):
                        Address = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.birthdate):
                        Birthdate = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.email):
                        Email = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.email_verified):
                        EmailVerified = bool.Parse(claim.Value?.ToString() ?? "false");
                        break;
                    case nameof(StandarClaims.family_name):
                        FamilyName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.gender):
                        Gender = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.given_name):
                        GivenName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.locale):
                        Locale = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.middle_name):
                        MiddleName = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.name):
                        Name = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.nickname):
                        Nickname = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.phone_number):
                        PhoneNumber = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.phone_number_verified):
                        PhoneNumberVerified = bool.Parse(claim.Value?.ToString() ?? "false");
                        break;
                    case nameof(StandarClaims.picture):
                        Picture = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.preferred_username):
                        PreferredUsername = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.profile):
                        Profile = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.sub):
                        Sub = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.updated_at):
                        UpdatedAt = !string.IsNullOrEmpty(claim.Value?.ToString()) ? Convert.ToDateTime(claim.Value?.ToString()) : new DateTime();
                        break;
                    case nameof(StandarClaims.website):
                        Website = claim.Value?.ToString();
                        break;
                    case nameof(StandarClaims.zoneinfo):
                        Zoneinfo = claim.Value?.ToString();
                        break;
                }

            }

        }

    }
}
