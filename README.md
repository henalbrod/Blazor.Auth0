
![alt-text](https://raw.githubusercontent.com/Pegazux/Blazor.Auth0/master/src/Blazor.Auth0.ClientSide/icon.png "Blazor.Auth0")

# Blazor Auth0 Library

This is a library for Blazor authentication with OIDC Authorization Code-Grant and Implicit-Grant flows, using Auth0's Universal Login and Silent Login for [Blazor](http://blazor.net) v3.0.0-preview5+ client & server side solutions, the idea behind this is to have an easy way of using Auth0's services in Blazor without the need of the auth0.js library.


## Start using it in 3 simple steps!


1) Start by adding a reference to Blazor-Auth0-ClientSide.0.5.1-alpha-2 for client side and Blazor-Auth0-ServerSide.0.2.1-alpha-2 for server side to your Blazor project

### Client Side

```
Install-Package Blazor-Auth0-ClientSide -Version 0.5.1-alpha-2
````

### Server Side

```
Install-Package Blazor-Auth0-ServerSide -Version 0.2.1-alpha-2
````


2) In Startup.cs, register the service inside ConfigureServices method


```C#
        public void ConfigureServices(IServiceCollection services)
        {
            // Uncomment for Server Side implementations
            // services.AddScoped<HttpClient>();

            services.AddScoped((sp) =>
            {
                return new Blazor.Auth0.Shared.Models.ClientSettings()
                {
                    Auth0Domain = "[Auth0_Domain]",
                    Auth0ClientId = "[Auth0_Client_Id]"
                };
            });

            services.AddScoped<Blazor.Auth0.[ClientSide|ServerSide].Authentication.AuthenticationService>();
        }
```


3) Replace the content of *MainLayout.razor* with the following code


```C#
@using Blazor.Auth0.Shared.Models.Enumerations
@using Blazor.Auth0.[ClientSide|ServerSide].Components
@using Blazor.Auth0.[ClientSide|ServerSide].Authentication

@inherits LayoutComponentBase

@inject AuthenticationService _authService

<AuthComponent>

    @*Will render while determining user's session state*@
    <UndefinedSessionStateContent>
        Determining session state, please wait...
    </UndefinedSessionStateContent>

    @*Will render after determining session state*@
    <ActiveInactiveSessionStateContent>

        <div class="sidebar">
            <NavMenu />
        </div>

        <div class="main">
            <div class="top-row px-4">
                @if (_authService.SessionState == SessionStates.Active)
                {
                    <a href="" class="ml-md-auto" onclick="@_authService.LogOut">LogOut</a>
                }
                else
                {
                    <a href="" class="ml-md-auto" onclick="@_authService.Authorize">LogIn</a>
                }
            </div>
            <div class="content px-4">
                @Body
            </div>
        </div>

    </ActiveInactiveSessionStateContent>

    @*There's also options for showing content on Active & Inactive session states alone*@
    <ActiveSessionStateContent></ActiveSessionStateContent>
    <InactiveSessionStateContent></InactiveSessionStateContent>

</AuthComponent>
```


### Other options include:

* **AuthenticationGrant**:  Allows you to choose between authorization_code (recommended) and implicit_grant authentication flows.

* **RedirectAlwaysToHome**: When set to true, forces the redirect_uri param to be the home path, this value overrides *Auth0RedirectUri*

* **LoginRequired**: When set to true, forces a redirection to the login page in case the user is not authenticated.

* **GetUserInfoFromIdToken**: When set to true, the serivce will use the id_token payload to build the user info, this is good in case all the user info you require is present in the id_token payload and you want avoid doing an extra call to Auth0, in case that tere's no id_token present in the authentication response the service will fall back gracefully to try to get the user info from an API call to auth0, default value is *false*
