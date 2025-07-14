using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalog.Interfaces;

public interface ITokenService
{
    // Vai me retornar um token Jwt.
    JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config); // Vou receber uma lista de Claims, que são informações sobre o usuario, e uma instância de IConfiguration para que eu possa acessar e ler informações do arquivo de config AppSettings.Json.
    string GenerateRefreshToken();

    // Para extrair as informações das Claims do Token gerado.
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config);
}
