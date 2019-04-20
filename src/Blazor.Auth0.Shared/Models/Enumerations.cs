using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Shared.Models.Enumerations
{
    public enum SessionStates
    {

        Undefined = 0,
        Active = 1,
        Inactive = 2

    }
    public enum StandarClaims {

        sub = 0,
        name,
        given_name,
        family_name,
        middle_name,
        nickname,
        preferred_username,
        profile,
        picture,
        website,
        email,
        email_verified,
        gender,
        birthdate,
        zoneinfo,
        locale,
        phone_number,
        phone_number_verified,
        address,
        updated_at

    }

    public enum AuthenticationGrantTypes {

        // -- Spec-conforming grants --
        /// <summary>
        /// New recomended authentication grant flow for SPAs as per OAuth2 working group [https://auth0.com/blog/oauth2-implicit-grant-and-spa/]
        /// </summary>
        authorization_code = 0,
        /// <summary>
        /// Previous recommended authentication grant flow for SPAs, now replaced with authorization_code as per OAuth2 working group [https://auth0.com/blog/oauth2-implicit-grant-and-spa/]
        /// </summary>
        implicit_grant,
        //client_credentials,
        //password
        //refresh_token // <-- Should I bother? this is a bad Idea in client side, maybe for server side

        // -- Auth0 extension grants --
        //password-realm, //http://auth0.com/oauth/grant-type/password-realm	
        //mfa-oob, //http://auth0.com/oauth/grant-type/mfa-oob
        //mfa-otp, //http://auth0.com/oauth/grant-type/mfa-otp
        //mfa-recovery-code //http://auth0.com/oauth/grant-type/mfa-recovery-code

        //No Legacy grant will be supported

    }

}
