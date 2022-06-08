using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace Terradue.Stars.Interface
{
    public interface IIdentityProvider
    {
        string Name { get; }

        JwtSecurityToken GetIdToken();

        ClaimsPrincipal GetPrincipal();
    }
}