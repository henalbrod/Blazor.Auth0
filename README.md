# Blazor.Auth0

<img src="https://raw.githubusercontent.com/henalbrod/Blazor.Auth0/master/src/Blazor.Auth0.ClientSide/icon.png" height="150" alt="Blazor Auth0 Library" align="right"/>

This is a library for Blazor authentication with OIDC Authorization Code-Grant and Implicit-Grant flows, using Auth0's Universal Login and Silent Login for [Blazor](http://blazor.net) over .NET Core v3.0.0-RC1 client & server-side solutions, the idea behind this is to have an easy way of using Auth0's services in Blazor without the need of the auth0.js library.

[![GitHub license](https://img.shields.io/github/license/henalbrod/Blazor.Auth0?color=blue)](https://github.com/henalbrod/Blazor.Auth0/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Blazor-Auth0-ServerSide?color=green&label=Nuget%3A%20Blazor-Auth0-ServerSide)](https://www.nuget.org/packages/Blazor-Auth0-ServerSide)
[![Nuget](https://img.shields.io/nuget/v/Blazor-Auth0-ClientSide?color=green&label=Nuget%3A%20Blazor-Auth0-Clientside)](https://www.nuget.org/packages/Blazor-Auth0-ClientSide)
[![Github Actions](https://github.com/henalbrod/Blazor.Auth0/workflows/Github%20Actions%20CI/badge.svg)](https://github.com/henalbrod/Blazor.Auth0/actions)


# About Auth0
Auth0 is a platform that provides authentication and authorization as a service. Giving developers and companies the building blocks they need to secure their applications without having to become security experts.

You can connect any application (written in any language or on any stack) to Auth0 and define the identity providers you want to use (how you want your users to log in).

Learn more at:

[<img width="150" height="50" alt="JWT Auth for open source projects" src="https://cdn.auth0.com/oss/badges/a0-badge-dark.png">](https://auth0.com/?utm_source=oss&utm_medium=gp&utm_campaign=oss)

## Prerequisites

### Blazor

>You'll want to follow the [Getting Started](https://docs.microsoft.com/en-us/aspnet/core/blazor/get-started?view=aspnetcore-3.0&tabs=visual-studio) instructions in [Blazor website](https://blazor.net)

### Auth0

> Basic knowledge of Auth0 IDaaS platform is assumed, otherwise, visiting [Auth0 docs](https://auth0.com/docs) is highly recommended.

## Installation

Install via [NPM](https://www.nuget.org/).

>Server Side
```bash
Install-Package Blazor-Auth0-ServerSide -Version 1.0.0-Preview2
````

>Client Side
```bash
Install-Package Blazor-Auth0-ClientSide -Version 1.0.0-Preview2
````

## Usage

 **Note**: Following example is for a server-side with require authenticated user implementation, for client-side and core-hosted example implementations please refer to the [examples](https://github.com/henalbrod/Blazor.Auth0/tree/master/examples)

> #### appsettings.json or [Secrets file](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.0&tabs=windows#secret-manager) (recommended)

```Json
{
	"Auth0":{
		"Domain": "[Your_Auth0_Tenant_Domain]",
		"ClientId": "[Your_Auth0_Client/Application_Id]",
		"ClientSecret": "[Your_Auth0_Client/Application_Secret]",
		"Audience": "[Your_Auth0_Audience/API_Identifier]"
	}
}
```
> #### Startup.cs

```C#
// Import Blazor.Auth0
using Blazor.Auth0;
using Blazor.Auth0.Models;
// ...

public void ConfigureServices(IServiceCollection services)
{
	// Other code...

	/// This one-liner will initialize Blazor.Auth0 with all the defaults
	services.AddDefaultBlazorAuth0Authentication(Configuration.GetSection("Auth0").Get<ClientOptions>());	

	// Other code...
}

 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
 {
    // Otrher code...

	app.UseHttpsRedirection();
	app.UseStaticFiles();
     
	// Add Blazor.Auth0 middleware     
	app.UseBlazorAuth0();

	// Other code...
 }
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

## Authors
**Henry Alberto Rodriguez** - _Initial work_ - [GitHub](https://github.com/henalbrod) -  [Twitter](https://twitter.com/henalbrod)  - [Linkedin](https://www.linkedin.com/in/henalbrod/)

* Especial thanks for its contributions to:

**jbomhold3** [GitHub](https://github.com/jbomhold3)
**TopSwagCode** [GitHub](https://github.com/TopSwagCode)

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/henalbrod/Blazor.Auth0/blob/master/LICENSE) file for details.

## Acknowledgments

* I started this library based on [this great post](https://auth0.com/blog/oauth2-implicit-grant-and-spa/) from [Vittorio Bertocci](https://twitter.com/vibronet)

* This README file is based on the great examples form: [makeareadme](https://www.makeareadme.com/), [PurpleBooth](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2) & [dbader](https://github.com/dbader/readme-template/blob/master/README.md)

## Release History

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