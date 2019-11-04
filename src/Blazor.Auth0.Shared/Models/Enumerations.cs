// <copyright file="Enumerations.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models.Enumerations
{
    public enum SessionStates
    {
        Undefined = 0,
        Active = 1,
        Inactive = 2,
    }

    public enum StandarClaims
    {
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
        updated_at,
    }

    public enum ResponseTypes
    {
        Code = 0,
        Token,
        IdToken,
        TokenAndIdToken,
    }

    public enum ResponseModes
    {
        Web_Message = 0,
        Query,
        Fragment,
        Form_Post,
    }

    public enum CodeChallengeMethods
    {
        None = 0,
        S256,
    }

    public enum LoginModes
    {
        Redirect = 0,
        Popup
    }
}
