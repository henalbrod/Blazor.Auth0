# Blazor (client-side) Auth0 Library

This is a library for Blazor authentication using Auth0 Authorization Code Grant Flow, Universal Login &amp; Silent Login for [Blazor](http://blazor.net) v0.9+ client side solutions, the idea behind this is to have an easy way of using Auth0's services in Blazor without the need of the auth0.js library.


### How to use it?

Start by adding a reference to Blazor.Auth0.0.1.0-alpha-1 to your Blazor Client Side project [Nuget Comming soon]

In Startup.cs:

Add a new line inside ConfigureServices method

```C#
  public void ConfigureServices(IServiceCollection services)
  {
      //...
      services.AddSingleton<Auth0.Authentication.AuthenticationService>();
      //...
  }
```


Add the following inside the Configure method

 ```C#
  public void Configure(IComponentsApplicationBuilder app, IUriHelper uriHelper)
  {

      var authenticationService = app.Services.GetService<Auth0.Authentication.AuthenticationService>();

      // OPTIONAL: Uncomment following line to force the user to be authenticated
      //authenticationService.LoginRequired = true;

      // REQUIRED: Indicate the Auth0's tenant & client information
      authenticationService.Auth0Settings = new Auth0.Models.Auth0Settings()
      {
          Domain = "[AUTH0_DOMAIN]",
          ClientId = "[AUTH0_CLIENT_ID]",
          // OPTIONAL: Comment following line to redirect to current path:
          RedirectUri = new Uri(uriHelper.GetAbsoluteUri()).GetLeftPart(System.UriPartial.Authority),
          Scope = "openid profile email"
      };

      // REQUIRED: Initializes the service and validates user's session state.
      authenticationService.ValidateSession();

      //...

      app.AddComponent<App>("app");
      
  }
  ```
  

3) Add a tag helper in _ViewImports.cshtml

```C#
@using Blazor.Auth0;
@addTagHelper *, Blazor.Auth0
```


4) Replace MainLayout.cshtml with the following

```C#
@inherits LayoutComponentBase
@inject Auth0.Authentication.AuthenticationService _authService

<WebAuthComponent>

  @*Will render while determining user's session state*@
  <UndefinedSessionContent>
      Determining session state, please wait...
  </UndefinedSessionContent>

  @*Will render after determining session state*@
  <ActiveInactiveSessionContent>

      <div class="sidebar">
          <NavMenu />
      </div>

      <div class="main">
          <div class="top-row px-4">
          @if (_authService.SessionState == Auth0.Models.Enumerations.SessionStates.Active)
          {
            <div class="top-row px-4">
                @if (_authService.SessionState == Auth0.Models.Enumerations.SessionStates.Active)
                {
                    <a href="" class="ml-md-auto" onclick="@_authService.LogOut">LogOut</a>
                }
                else
                {
                    <a href="" class="ml-md-auto" onclick="@_authService.LogIn">LogIn</a>
                }
            </div>
          }
          </div>
          <div class="content px-4">
              @Body
          </div>
      </div>

  </ActiveInactiveSessionContent>

  @*There's also options for showing content on Active & Inactive session state alone*@
  <ActiveSessionContent></ActiveSessionContent>
  <InactiveSessionContent></InactiveSessionContent>

</WebAuthComponent>
```


### Known issues

- Only Code-Grant Flow tested
- No server-side support yet
