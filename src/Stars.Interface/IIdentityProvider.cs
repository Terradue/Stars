using System.IdentityModel.Tokens.Jwt;

namespace Terradue.Stars.Interface
{
    public interface IIdentityProvider
    {
        string Name { get; }

        JwtSecurityToken GetJwtSecurityToken();
    }
}