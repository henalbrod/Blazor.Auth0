// <copyright file="BlazorAuth0BuilderExtensions.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using Blazor.Auth0.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Class containing extension methods for Blazor.Auth0 default initialization.
    /// </summary>
    public static class BlazorAuth0BuilderExtensions
    {
        /// <summary>
        /// Add Blazor.Auth0 default services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="Action"/> containing a <see cref="ClientOptions"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0(this IServiceCollection services, Action<ClientOptions> options)
        {
            services.AddBlazorAuth0ClientOptions(options);

            return services.AddBlazorAuth0();
        }

        /// <summary>
        /// Add Blazor.Auth0 default services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0(this IServiceCollection services)
        {
            services.AddScoped<Blazor.Auth0.IAuthenticationService, Blazor.Auth0.AuthenticationService>();
            services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, Blazor.Auth0.AuthenticationStateProvider>();

            return services;
        }

        /// <summary>
        /// Add Blazor.Auth0 client options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="Action"/> containing a <see cref="ClientOptions"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0ClientOptions(this IServiceCollection services, Action<ClientOptions> options = null)
        {
            services.Configure(options);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ClientOptions>>().Value);

            return services;
        }

        /// <summary>
        /// Add Blazor.Auth0 client options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="ClientOptions"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0ClientOptions(this IServiceCollection services, ClientOptions options = null)
        {
            services.AddSingleton(resolver => options ?? resolver.GetRequiredService<IOptions<ClientOptions>>().Value);
            return services;
        }
    }
}
