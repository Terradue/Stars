// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IIdentityProvider.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Terradue.Stars.Interface
{
    public interface IIdentityProvider
    {
        string Name { get; }

        JwtSecurityToken GetIdToken();

        ClaimsPrincipal GetPrincipal();

    }
}
