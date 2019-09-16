// <copyright file="ParseHashOptions.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Blazor.Auth0.Models.Enumerations;

    /// <summary>
    /// Class for handling the options required for parshing a hash.
    /// </summary>
    public class ParseHashOptions : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the Uri containing the hash. If not provided it will extract from window.location.hash.
        /// </summary>
        [Required]
        public Uri AbsoluteUri { get; set; }

        /// <summary>
        /// Gets or sets type of the response used by OAuth 2.0 flow. It can be any space separated list of the values `token`, `id_token`.
        /// For this specific method, we'll only use this value to check if the hash contains the tokens requested in the responseType.
        /// </summary>
        public ResponseTypes ResponseType { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            return results;
        }
    }
}
