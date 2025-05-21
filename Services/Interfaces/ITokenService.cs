using CustomerService.API.Models;
using System.Security.Claims;

namespace CustomerService.API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}
