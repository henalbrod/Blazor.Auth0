using Microsoft.JSInterop;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Blazor.Auth0.Shared.Models;
using Blazor.Auth0.Shared.Authentication;
using System.Threading.Tasks;

namespace Blazor.Auth0.ServerSide.Authentication
{
    public class AuthenticationService : AuthenticationServiceBase
    {
        public AuthenticationService(
            IComponentContext componentContext,
            HttpClient httpClient,
            IJSRuntime jsRuntime,
            IUriHelper uriHelper,
            ClientSettings settings) :
        base(
            componentContext,
            httpClient,
            jsRuntime,
            uriHelper,
            settings
        ) { }


        /// <summary>
        /// Meant for internal API use only.
        /// </summary>
        [JSInvokable]
        public override Task HandleAuth0Message(Auth0IframeMessage message)
        {
            return base.HandleAuth0Message(message);
        }

    }
}