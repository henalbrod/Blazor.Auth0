using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Auth0.Models.Enumerations
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
}
