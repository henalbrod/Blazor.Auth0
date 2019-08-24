// <copyright file="SessionAuthorizationTransaction.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models
{
    /// <summary>
    /// Class for representing an authentication flow parameters.
    /// </summary>
    public class SessionAuthorizationTransaction
    {
        /// <summary>
        /// Gets or sets the transaction's state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the transaction's nonce.
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the transaction's code_verifier.
        /// </summary>
        public string CodeVerifier { get; set; }

        /// <summary>
        /// Gets or sets the transaction's app state.
        /// </summary>
        public string AppState { get; set; }

        /// <summary>
        /// Gets or sets the transaction's redirec_uri.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the transaction's connection.
        /// </summary>
        public string Connnection { get; set; }
    }
}
