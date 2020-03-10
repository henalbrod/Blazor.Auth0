# Blazor.Auth0

<img src="https://raw.githubusercontent.com/henalbrod/Blazor.Auth0/master/src/Blazor.Auth0.ClientSide/icon.png" height="150" alt="Blazor Auth0 Library" align="right"/>

[Blazor.Auth0](https://github.com/henalbrod/Blazor.Auth0) Is a library for using the [Authorization Code Grant with Proof Key for Code Exchange (PKCE)](https://auth0.com/blog/oauth2-implicit-grant-and-spa/) with [Auth0's Universal Login](https://auth0.com/docs/libraries/when-to-use-lock) in Blazor SPAs.

The idea behind this is to have an easy way of using Auth0's services with Blazor (especially the client side) without relaying on javascript libraries.

[![Nuget](https://img.shields.io/nuget/v/Blazor-Auth0-ServerSide?color=green&label=Nuget%3A%20Blazor-Auth0-ServerSide)](https://www.nuget.org/packages/Blazor-Auth0-ServerSide)
[![Nuget](https://img.shields.io/nuget/v/Blazor-Auth0-ClientSide?color=green&label=Nuget%3A%20Blazor-Auth0-Clientside)](https://www.nuget.org/packages/Blazor-Auth0-ClientSide)
[![Github Actions](https://github.com/henalbrod/Blazor.Auth0/workflows/Github%20Actions%20CI/badge.svg)](https://github.com/henalbrod/Blazor.Auth0/actions)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/675aa31ceb9a4898be281c23423ca134)](https://www.codacy.com/manual/henalbrod/Blazor.Auth0?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=henalbrod/Blazor.Auth0&amp;utm_campaign=Badge_Grade)
[![GitHub license](https://img.shields.io/github/license/henalbrod/Blazor.Auth0?color=blue)](https://github.com/henalbrod/Blazor.Auth0/blob/master/LICENSE)


# About Auth0
Auth0 is a platform that provides authentication and authorization as a service. Giving developers and companies the building blocks they need to secure their applications without having to become security experts.

You can connect any application (written in any language or on any stack) to Auth0 and define the identity providers you want to use (how you want your users to log in).

Learn more at:

[<img width="150" height="50" alt="JWT Auth for open source projects" src="https://cdn.auth0.com/oss/badges/a0-badge-dark.png">](https://auth0.com/?utm_source=oss&utm_medium=gp&utm_campaign=oss)

## Prerequisites

### Blazor

>You'll want to follow the [Getting Started](https://docs.microsoft.com/en-us/aspnet/core/blazor/get-started?view=aspnetcore-3.1&tabs=visual-studio) instructions in [Blazor website](https://blazor.net)

### Auth0

> Basic knowledge of Auth0 IDaaS platform is assumed, otherwise, visiting [Auth0 docs](https://auth0.com/docs) is highly recommended.

## Installation

Install via [Nuget](https://www.nuget.org/).

>Server Side
```bash
Install-Package Blazor-Auth0-ServerSide -Version 2.0.0-Preview5
````

>Client Side
```bash
Install-Package Blazor-Auth0-ClientSide -Version 2.0.0-Preview5
````

## Usage

 **Note**: Following example is for a client-side with require-authenticated-user implementation, for server-side and core-hosted example implementations please refer to the [examples](https://github.com/henalbrod/Blazor.Auth0/tree/master/examples)


> #### Program.cs

```C#

using Blazor.Auth0;

// ...


public static async Task Main(string[] args)
{
	var builder = WebAssemblyHostBuilder.CreateDefault(args);

	builder.Services.AddBlazorAuth0(options =>
	{
		// Required
		options.Domain = "[Auth0_Domain]";

		// Required
		options.ClientId = "[Auth0_Client_Id]";

		//// Required if you want to make use of Auth0's RBAC
		options.Audience = "[Auth0_Audience]";

		//// Uncomment the following line if you don't want your users to be automatically logged-off on token expiration
		// options.SlidingExpiration = true;

		//// Uncomment the following two lines if you want your users to log in via a pop-up window instead of being redirected
		// options.LoginMode = LoginModes.Popup;

		//// Uncomment the following line if you don't want your unauthenticated users to be automatically redirected to Auth0's Universal Login page 
		// options.RequireAuthenticatedUser = false;
	});
	
	builder.Services.AddAuthorizationCore();

	builder.RootComponents.Add<App>("app");

	await builder.Build().RunAsync();
}

```

###
Add a reference to Microsoft.AspNetCore.Components.Authorization
> #### _Imports.razor

```C#
@using Microsoft.AspNetCore.Components.Authorization
//...
```

###
Replace App.razor content with the following code
> #### App.razor

```HTML
<Router AppAssembly="@typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
            <Authorizing>
                <p>>Determining session state, please wait...</p>
            </Authorizing>
            <NotAuthorized>
                <h1>Sorry</h1>
                <p>You're not authorized to reach this page. You may need to log in as a different user.</p>
            </NotAuthorized>
        </AuthorizeRouteView>
    </Found>
    <NotFound>        
        <p>Sorry, there's nothing at this address.</p>        
    </NotFound>
</Router>
```
## Support
If you found a bug, have a consultation or a feature request please feel free to [open an issue](https://github.com/henalbrod/Blazor.Auth0/issues).

When opening issues please take in account to:

* **Avoid duplication**: Please search for similar issues before.
* **Be specific**: Please don't put several problems/ideas in the same issue.
* **Use short descriptive titles**: You'll have the description box to explain yourself.
* **Include images whenever possible**: A picture is worth a thousand words.
* **Include reproduction steps for bugs**:  Will be appreciated

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

1.  Fork it ([https://github.com/henalbrod/Blazor.Auth0/fork](https://github.com/henalbrod/Blazor.Auth0/fork))
2.  Create your feature branch (`git checkout -b feature/fooBar`)
3.  Commit your changes (`git commit -am 'Add some fooBar'`)
4.  Push to the branch (`git push origin feature/fooBar`)
5.  Create a new Pull Request

* Especial thanks for its help to all the [contributors](https://github.com/henalbrod/Blazor.Auth0/graphs/contributors)

## Authors
**Henry Alberto Rodriguez** - _Initial work_ - [GitHub](https://github.com/henalbrod) -  [Twitter](https://twitter.com/henalbrod)  - [Linkedin](https://www.linkedin.com/in/henalbrod/)

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/henalbrod/Blazor.Auth0/blob/master/LICENSE) file for details.

## Acknowledgments

* I started this library based on [this great post](https://auth0.com/blog/oauth2-implicit-grant-and-spa/) from [Vittorio Bertocci](https://twitter.com/vibronet)

* This README file is based on the great examples form: [makeareadme](https://www.makeareadme.com/), [PurpleBooth](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) & [dbader](https://github.com/dbader/readme-template/blob/master/README.md)

## Release History

**v2.0.0-Preview5**

* Fixed issue #41
* Upgraded to .Net Core v3.1.102

**v2.0.0-Preview4**

* Upgraded to .Net Core v3.1.0-preview3

**v2.0.0-Preview3**

* Upgraded to .Net core 3.1.0-preview2


**v2.0.0-Preview2**

This relase comes with Client Side changes primarly

* New LoginMode parameter in ClientOptions 

  Redirect = Classic behavior (default)
  PopUp = Loads Universal Login inside a popup window
  
  The new PopUp behavior comes in handy to avoid the full client side app reloading

* New AuthorizePopup method in Blazor.Auth0.Authentication for client side

**v2.0.0-Preview1**

BREAKING CHANGES:

* Upgraded to .Net Core 3.1.0-preview1
* Server side projects upgraded to netcoreapp3.1
* Auth0 permissions are now accesible as an any other array claim:
```C#
policy.RequireClaim("permissions", "permission_name")
```

**v1.0.0-Preview3**
* Overall upgrade to .Net Core 3.0

**v1.0.0-Preview2**
* Overall upgrade to .Net Core 3.0 RC1
* Removed Shell.razor in Example projects
* Simplified App.razor in Example projects
* Removed local _imports.razor in Example projects

**v0.1.0.0-Preview1**
* Upgraded to .Net Core 3.0.0-preview8
* Removed AuthComponent
* New One-Liner instantiation
* Server Side full rewrite
	* Better server-side Blazor Authentication compatibility/integration
	* Cookie-based session (No more silent login iframe in server-side)
	* Refresh token support (Refreshing and Revoking)
	* Client secret
	* Server-side sliding expiration
