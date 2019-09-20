// <copyright file="Enumerations.cs" company="Henry Alberto Rodriguez Rodriguez">
// Copyright (c) 2019 Henry Alberto Rodriguez Rodriguez
// </copyright>

namespace Blazor.Auth0.Models.Enumerations
{
    public enum Endpoint
    {
        Authorize = 0,
        Token = 1,
        UserInfo = 2,
        Device_Authorization = 3,
        Introspection = 4,
        Revocation = 5,
        End_Session = 6
    }

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

    public enum RequestModes
    {
        Json = 0,
        Form_Post
    }

    public enum CodeChallengeMethods
    {
        None = 0,
        S256,
    }
}
