// <copyright file="Utils.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Seconds since epoch.
        /// </summary>
        public static long Epoch => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        /// <summary>
        /// Validates an IValidatable object.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="item">The object to be validated.</param>
        public static void ValidateObject<T>(T item)
            where T : IValidatableObject
        {
            ValidationContext context = new ValidationContext(item, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(item, context, results);

            results.AddRange(item.Validate(context));

            if (!isValid)
            {
                var stringBuilder = new StringBuilder();

                foreach (ValidationResult validationResult in results.Where(x => x != null))
                {
                    stringBuilder.Append($"{validationResult.ErrorMessage}; ");
                }

                ValidationException ex = new ValidationException($"Invalid {item.GetType().Name}: {stringBuilder}");

                throw ex;
            }
        }

        /// <summary>
        /// Gets the base64 representation of an string.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>A base64 string representation of an string.</returns>
        public static string GetSha256(string value)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(value));
                return Convert.ToBase64String(hashValue).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }
        }
    }
}
