// <copyright file="ScopeValidation.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class to handle scope validation methods.
    /// </summary>
    public static class ScopeValidation
    {

        /// <summary>
        /// Validates an scope string.
        /// </summary>
        /// <param name="scope">The scope string</param>
        /// <returns>A <see cref="ValidationResult"/>.</returns>
        public static ValidationResult ScopeValidate(string scope)
        {
            return !string.IsNullOrEmpty(scope) && scope.Contains("openid")
                ? ValidationResult.Success
                : new ValidationResult("Scope is not valid: must contain 'openid'");
        }
    }
}
