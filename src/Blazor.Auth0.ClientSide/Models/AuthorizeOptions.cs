// <copyright file="AuthorizeOptions.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Blazor.Auth0.Models.Enumerations;

    /// <summary>
    /// Class for handling the options required in the authentication flow.
    /// </summary>
    public class AuthorizeOptions : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the Auth0's tenant domain used in the authentication flow.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Auth0's tenant client id used in the authentication flow.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string ClientID { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect the user after the user authentication.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ResponseType"/> used in the authentication flow.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public ResponseTypes ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ResponseModes"/> used in the authentication flow.
        /// </summary>
        public ResponseModes ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets the state used in the authentication flow.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the nonce used in the authentication flow.
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CodeChallengeMethods"/> used in the authentication flow.
        /// </summary>
        public CodeChallengeMethods CodeChallengeMethod { get; set; }

        /// <summary>
        /// Gets or sets the PKCE code challenge used in the authentication flow.
        /// </summary>
        public string CodeChallenge { get; set; }

        /// <summary>
        /// Gets or sets the PKCE code verifier used in the authentication flow.
        /// </summary>
        public string CodeVerifier { get; set; }

        /// <summary>
        /// Gets or sets the scope used in the authentication flow.
        /// </summary>
        [Required(ErrorMessage = "{0} option is required")]
        public string Scope { get; set; } = "openid profile email";

        /// <summary>
        /// Gets or sets the Auth0's Audience/API identifier used in the authentication flow.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the Auth0's connection used in the authentication flow.
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets the Auth0's realm used in the authentication flow.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the app state used in the authentication flow.
        /// </summary>
        public string AppState { get; set; }

        /// <summary>
        /// Gets or sets the namespace state used in the authentication flow.
        /// </summary>
        public object Namespace { get; set; }

        /// <summary>
        /// Gets or sets the length for the nonce and code callenges.
        /// </summary>
        public int KeyLength { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>
            {
                ScopeValidation.ScopeValidate(this.Scope),
            };

            if (this.CodeChallengeMethod != CodeChallengeMethods.None && string.IsNullOrEmpty(this.CodeChallenge))
            {
                results.Add(new ValidationResult($"{nameof(this.CodeChallenge)} is not valid: You're using code_challenge_method='{CommonAuthentication.ParseCodeChallengeMethod(this.CodeChallengeMethod)}' but no code_challenge is present"));
            }

            if (this.CodeChallengeMethod != CodeChallengeMethods.None && string.IsNullOrEmpty(this.CodeVerifier))
            {
                results.Add(new ValidationResult($"{nameof(this.CodeVerifier)} is not valid: You're using code_challenge_method='{CommonAuthentication.ParseCodeChallengeMethod(this.CodeChallengeMethod)}' but no code_verifier is present"));
            }

            if (this.ResponseType == ResponseTypes.IdToken && this.ResponseType == ResponseTypes.TokenAndIdToken && string.IsNullOrEmpty(this.Nonce))
            {
                results.Add(new ValidationResult($"{nameof(this.Nonce)} is not valid: Nonce is required when using Implicit Grant (id_token or token id_token response types)"));
            }

            return results;
        }
    }
}
