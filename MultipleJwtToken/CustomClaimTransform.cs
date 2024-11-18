using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MultipleJwtToken;

public class CustomClaimTransform (IConfiguration configuration): IClaimsTransformation
{
    private readonly string _authSource = "auth_source";
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.HasClaim(c => c.Type == _authSource))
        {
            return Task.FromResult(principal);
        }

        var claimsIdentity = new ClaimsIdentity();
        var issuer = principal.Identities.Select(i => i.FindFirst(JwtRegisteredClaimNames.Iss)?.Value)
            .FirstOrDefault();
        if (issuer == configuration["Auth:Supabase:ValidIssuer"])
        {
            claimsIdentity.AddClaim(new Claim(_authSource, CustomAuthSchemes.Supabase));
        }
        else if (issuer == configuration["Auth:Keycloak:ValidIssuer"])
        {
            claimsIdentity.AddClaim(new Claim(_authSource, CustomAuthSchemes.Keycloak));
        }

        principal.AddIdentity(claimsIdentity);
        return Task.FromResult(principal);
    }
}