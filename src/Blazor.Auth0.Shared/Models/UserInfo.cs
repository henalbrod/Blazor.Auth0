// <copyright file="UserInfo.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Blazor.Auth0.Models.Enumerations;

    /// <summary>
    /// Class for representing the user info.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets user's address.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.address))]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets user's date of birth.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.birthdate))]
        public string Birthdate { get; set; }

        /// <summary>
        /// Gets or sets user's email.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.email))]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email is verified or not.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.email_verified))]
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Gets or sets user's family name.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.family_name))]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets user's gender.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.gender))]
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets user's given name.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.given_name))]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets user's locale.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.locale))]
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets user's middle name.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.middle_name))]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets user's name.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.name))]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets user's nickname.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.nickname))]
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets user's phone number.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.phone_number))]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the users phone number is verified or not.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.phone_number_verified))]
        public bool PhoneNumberVerified { get; set; }

        /// <summary>
        /// Gets or sets user's picture URI.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.picture))]
        public string Picture { get; set; }

        /// <summary>
        /// Gets or sets user's preferred username.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.preferred_username))]
        public string PreferredUsername { get; set; }

        /// <summary>
        /// Gets or sets user's profile.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.profile))]
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets user's sub.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.sub))]
        public string Sub { get; set; }

        /// <summary>
        /// Gets or sets user's update date.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.updated_at))]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets user's website URI.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.website))]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets user's zone info.
        /// </summary>
        [JsonPropertyName(nameof(StandarClaims.zoneinfo))]
        public string Zoneinfo { get; set; }

        /// <summary>
        /// Gets or sets the nonce used in user's authentication flow.
        /// </summary>
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets user's access token hash used in user's authentication flow.
        /// </summary>
        [JsonPropertyName("at_hash")]
        public string AtHash { get; set; }

        /// <summary>
        /// Gets or sets user's permissions claims.
        /// </summary>
        [JsonIgnore]
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets user's custom claims.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> CustomClaims { get; set; } = new Dictionary<string, JsonElement>();
    }
}
