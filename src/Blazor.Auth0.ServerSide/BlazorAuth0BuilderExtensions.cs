// <copyright file="BlazorAuth0BuilderExtensions.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
    using Blazor.Auth0.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Extension methods for Blazor.Auth0 default initialization.
    /// </summary>
    public static class BlazorAuth0BuilderExtensions
    {
        /// <summary>
        /// Add default Blazor.Auth0 authentication.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="ClientOptions"/> instance.</param>
        public static void AddDefaultBlazorAuth0Authentication(this IServiceCollection services, ClientOptions options)
        {
            services.AddBlazorAuth0ClientOptions(options);
            services.AddDefaultBlazorAuth0Authentication();
        }

        /// <summary>
        /// Add default Blazor.Auth0 authentication.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        public static void AddDefaultBlazorAuth0Authentication(this IServiceCollection services)
        {
            // TODO: This method is too convulted, it should be separeted into smaller pieces
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            ClientOptions clientOptions = services.BuildServiceProvider().GetRequiredService<ClientOptions>();

            Utils.ValidateObject(clientOptions);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = clientOptions.SlidingExpiration;
            })
            .AddBlazorAuth0()
            .AddOpenIdConnect(clientOptions.ClaimsIssuer, options =>
            {
                // Set the authority to your Auth0 domain
                options.Authority = $"https://{clientOptions.Domain}";
                options.RequireHttpsMetadata = false;

                // Configure the Auth0 Client ID and Client Secret
                options.ClientId = clientOptions.ClientId;
                options.ClientSecret = clientOptions.ClientSecret;

                options.ResponseType = CommonAuthentication.ParseResponseType(clientOptions.ResponseType);

                string[] scopes = clientOptions.Scope.Trim().ToLowerInvariant().Split(",");

                options.Scope.Clear();

                foreach (string scope in scopes)
                {
                    options.Scope.Add(scope);
                }

                if (clientOptions.ResponseType == Models.Enumerations.ResponseTypes.Code && !string.IsNullOrEmpty(clientOptions.ClientSecret))
                {
                    options.Scope.Add("offline_access");
                }

                options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.CallbackPath = new PathString(clientOptions.CallbackPath);
                options.RemoteSignOutPath = new PathString(clientOptions.RemoteSignOutPath);

                options.SignedOutRedirectUri = clientOptions.RedirectUri;

                options.ClaimsIssuer = clientOptions.ClaimsIssuer;

                options.UseTokenLifetime = !clientOptions.SlidingExpiration;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateTokenReplay = true,
                    ValidIssuers = new string[]
                    {
                        options.Authority,
                    },
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                if (!string.IsNullOrEmpty(clientOptions.Audience))
                {
                    options.TokenValidationParameters.ValidAudiences = new string[] {
                        clientOptions.Audience,
                        $"{options.Authority}/userinfo",
                    };
                }

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (!string.IsNullOrEmpty(clientOptions.Audience))
                        {
                            context.ProtocolMessage.SetParameter("audience", clientOptions.Audience);

                            HttpRequest request = context.Request;
                            var errorUri = request.Scheme + "://" + request.Host + request.PathBase;
                            context.ProtocolMessage.SetParameter("error_uri", errorUri);
                        }

                        return Task.FromResult(0);
                    },

                    OnRemoteSignOut = (context) =>
                    {
                        Authentication.ClearAspNetCookies(context.HttpContext);

                        string redirectUri = context.Request.Query
                                            .Where(x => x.Key.ToLowerInvariant() == "redirect_uri")
                                            .Select(x => x.Value)
                                            .FirstOrDefault();

                        if (string.IsNullOrEmpty(redirectUri))
                        {
                            HttpRequest request = context.Request;
                            redirectUri = request.Scheme + "://" + request.Host + request.PathBase;
                        }

                        string logoutUri = CommonAuthentication.BuildLogoutUrl(clientOptions.Domain, clientOptions.ClientId, redirectUri);

                        context.Response.Redirect(logoutUri);

                        context.HandleResponse();

                        return Task.CompletedTask;
                    },

                    // handle the logout redirection
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        string logoutUri = CommonAuthentication.BuildLogoutUrl(clientOptions.Domain, clientOptions.ClientId);

                        string postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                HttpRequest request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }

                            logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = (u) =>
                    {
                        if (!string.IsNullOrEmpty(clientOptions.Audience))
                        {
                            string accessToken = u.TokenEndpointResponse.AccessToken;
                            AccessTokenPayload parsedAccessToken = !string.IsNullOrEmpty(accessToken) ? CommonAuthentication.DecodeTokenPayload<AccessTokenPayload>(accessToken) : default;
                            List<string> permissions = parsedAccessToken?.Permissions ?? new List<string>();

                            if (permissions.Any())
                            {
                                string name = u.Principal.Claims.Where(x => x.Type == "name").FirstOrDefault()?.Value;
                                name ??= u.Principal.Claims.Where(x => x.Type == ClaimTypes.GivenName).FirstOrDefault()?.Value;
                                name ??= u.Principal.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
                                name ??= u.Principal.Claims.Where(x => x.Type == "nickname").FirstOrDefault()?.Value;
                                name ??= u.Principal.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;

                                GenericIdentity identity = new GenericIdentity(name, "JWT");

                                identity.AddClaims(u.Principal.Claims);

                                identity.AddClaims(permissions.Select(permission => new Claim("permissions", permission, "permissions")));

                                identity.AddClaim(new Claim("exp", parsedAccessToken.Exp.ToString(), ClaimValueTypes.Integer64));

                                ClaimsPrincipal user = new ClaimsPrincipal(identity);

                                u.Principal = user;
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAccessDenied = (u) =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = (u) =>
                    {
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = (context) =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },
                };
            });

        }

        /// <summary>
        /// Add Blazor.Auth0 default services.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static AuthenticationBuilder AddBlazorAuth0(this AuthenticationBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddBlazorAuth0();

            return builder;
        }

        /// <summary>
        /// Add Blazor.Auth0 default services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="ClientOptions"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0(this IServiceCollection services, ClientOptions options)
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
            services.AddHttpContextAccessor();

            services.AddSingleton<HttpClient>();
            services.AddScoped<Blazor.Auth0.IAuthenticationService, Blazor.Auth0.AuthenticationService>();
            services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, Blazor.Auth0.AuthenticationStateProvider>();

            return services;
        }

        /// <summary>
        /// Add Blazor.Auth0 client options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0ClientOptions(this IServiceCollection services)
        {
            return AddBlazorAuth0ClientOptions(services, null);
        }

        /// <summary>
        /// Add Blazor.Auth0 client options.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <param name="options">The <see cref="ClientOptions"/> instance.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddBlazorAuth0ClientOptions(this IServiceCollection services, ClientOptions options)
        {
            services.AddSingleton(resolver => options ?? resolver.GetRequiredService<IOptions<ClientOptions>>().Value);

            return services;
        }
    }
}
