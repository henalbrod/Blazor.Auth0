// <copyright file="BlazorAuth0Middleware.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0
{
    using System.Threading.Tasks;
    using Blazor.Auth0.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    public static class BlazorAuth0MiddlewareExtensions
    {
        public static IApplicationBuilder UseBlazorAuth0(this IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            return app.UseMiddleware<BlazorAuth0Middleware>();
        }
    }

    public class BlazorAuth0Middleware
    {
        private readonly RequestDelegate _next;

        public BlazorAuth0Middleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context, ClientOptions clientOptions)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (clientOptions is null)
            {
                throw new System.ArgumentNullException(nameof(clientOptions));
            }

            if (!context.User.Identity.IsAuthenticated && (clientOptions.RequireAuthenticatedUser || context.Request.Path == new PathString("/account/authorize")))
            {
                AuthenticationProperties authenticationProperties = new AuthenticationProperties { RedirectUri = !string.IsNullOrEmpty(clientOptions.RedirectUri) ? clientOptions.RedirectUri : "/" };
                Authentication.ClearAspNetCookies(context);
                await context.ChallengeAsync(clientOptions.ClaimsIssuer, authenticationProperties).ConfigureAwait(false);
            }
            else
            {
                await this._next(context);
            }
        }
    }
}
