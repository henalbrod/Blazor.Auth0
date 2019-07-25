
![alt-text](https://raw.githubusercontent.com/Pegazux/Blazor.Auth0/master/src/Blazor.Auth0.ClientSide/icon.png "Blazor.Auth0")

# Blazor Auth0 Library

This is a library for Blazor authentication with OIDC Authorization Code-Grant and Implicit-Grant flows, using Auth0's Universal Login and Silent Login for [Blazor](http://blazor.net) over .NET Core v3.0.0-preview7 client & server side solutions, the idea behind this is to have an easy way of using Auth0's services in Blazor without the need of the auth0.js library.


## Start using it in 3 simple steps!

**Note**: Following example will implement a "An authenticated user is Required" flow, for a "Login/Logout Buttons" flow please refer to [here](https://github.com/Pegazux/Blazor.Auth0/tree/master/examples/Blazor.Auth0.Examples.ServerSide)

1) Start by adding a reference to Blazor-Auth0-ClientSide.0.7.2-beta.2 for client side and Blazor-Auth0-ServerSide.0.4.2-beta.2 for server side to your Blazor project

### Client Side

```
Install-Package Blazor-Auth0-ClientSide -Version 0.7.2-beta.2
````

### Server Side

```
Install-Package Blazor-Auth0-ServerSide -Version 0.4.2-beta.2
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
                    Auth0ClientId = "[Auth0_Client_Id]",
					LoginRequired = true
                };
            });

            services.AddScoped<Blazor.Auth0.[ClientSide|ServerSide].Authentication.AuthenticationService>();
			
        }
```

### Other options include:

* **AuthenticationGrant**:  Allows you to choose between authorization_code (recommended) and implicit_grant authentication flows.

* **RedirectAlwaysToHome**: When set to true, forces the redirect_uri param to be the home path, this value overrides *Auth0RedirectUri*

* **LoginRequired**: When set to true, forces a redirection to the login page in case the user is not authenticated.

* **GetUserInfoFromIdToken**: When set to true, the serivce will use the id_token payload to build the user info, this is good in case all the user info you require is present in the id_token payload and you want avoid doing an extra call to Auth0, in case that tere's no id_token present in the authentication response the service will fall back gracefully to try to get the user info from an API call to auth0, default value is *false*



3) Replace the content of *App.razor* with the following code


```C#
@using Blazor.Auth0.Shared.Models.Enumerations
@using Blazor.Auth0.[ClientSide|ServerSide].Components
@using Blazor.Auth0.[ClientSide|ServerSide].Authentication

@inject AuthenticationService _authService

<AuthComponent ProtectedPaths="protectedPaths">

	@*Will render while determining user's session state*@
	<AuthorizingContent>
		Determining session state, please wait...
	</AuthorizingContent>

	@*Will render after determining session state*@
	<AuthorizedContent>

		<Router AppAssembly="typeof(Startup).Assembly">

			<NotFoundContent>
				<p>Sorry, there's nothing at this address.</p>
			</NotFoundContent>

		</Router>

	</AuthorizedContent>

	<NotAuthorizedContent>
		ERROR 401: Unauthorized
	</NotAuthorizedContent>

</AuthComponent>

@code {

    List<string> protectedPaths = new List<string> {
        "fetchdata"
    };

}
```

### Other options include:

* **ProtectedPaths**:  Allows you to indicate a list of paths that requires an authenticated user, only affects if LoginRequired is set to false.
* **LoginRequiredOnProtectedPaths**:  Alters the ProtectedPaths behavior. When set to true redirect the user to the login page automatically, otherwise, the content from the UnAuthorizedContent fragment will be rendered.


# Using it with the built-in Blazor Authentication and Authorization

Since v0.7.0-beta.1 for client side version and v0.4.0-beta.1 for server side version, you can make use of the new built-in Blazor Authentication and Authorization capabilities.

In Startup.cs, register a new AuthenticationStateProvider service inside ConfigureServices method

```C#

	services.AddScoped<AuthenticationStateProvider, Blazor.Auth0.[ClientSide|ServerSide].Authentication.AuthenticationStateProvider>();		
	
```

### Server Side Only!

In the Configure method of the same file, instructs the app to use Authentication and Authorization

```C#

	app.UseAuthentication();
	app.UseAuthorization();
	
```

### IMPORTANT!

If you're planning to use built-in Blazor Authorization capabilities like Claims-based and Policy-based authorization, then you will need to indicate an Auth0 audience/api with the "Enable RBAC" and "Add Permissions in the Access Token" settings set to true, here you have some reading about [Auth0's RBAC](https://auth0.com/docs/authorization/concepts/rbac).



> The actual mechanism of authenticating the user, i.e., determining their identity using cookies or other information, is the same in Blazor as in any other ASP.NET Core application. So to control and customize any aspect of it, see documentation about authentication in ASP.NET Core.
>> SteveSandersonMS

# Following are great sources of how to implement the Authentication and Authorization:

[ASP.NET Core and Blazor updates in .NET Core 3.0 Preview](https://devblogs.microsoft.com/aspnet/asp-net-core-and-blazor-updates-in-net-core-3-0-preview-6/),
[SteveSandersonMS/blazor-auth.md](https://gist.github.com/SteveSandersonMS/175a08dcdccb384a52ba760122cd2eda),
[Policy-based authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.0),
[Claims-based authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-3.0)

