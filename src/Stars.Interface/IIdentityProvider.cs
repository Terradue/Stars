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
