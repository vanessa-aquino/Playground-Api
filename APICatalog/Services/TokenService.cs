using APICatalog.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICatalog.Services;

public class TokenService : ITokenService
{
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration _config)
    {
        // Inicio obtendo a minha chave secreta.
        var key = _config.GetSection("JWT").GetValue<string>("SecretKey")
            ?? throw new InvalidOperationException("Invalid secret key");

        // Converto a chave para um array de Bytes, pois ela está em formato string.
        var privateKey = Encoding.UTF8.GetBytes(key);

        // Aqui eu estou criando as credenciais para assinar o token e uso o algoritmo HmacSha256Signature para assinar o token.
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature); // A classe SymmetricSecurityKey é usada em conjunto com a SigningCredentials para configurar a chave de assinatura necessária para verificar a autenticidade de tokens JWt. 

        // Aqui estou descrevendo as informações que vou usar para gerar o token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), // Obtendo as claims do user.
            Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")), // Definindo a data de expiração.
            Audience = _config.GetSection("JWT").GetValue<string>("ValidAudience"), // Obtendo a audiencia. 
            Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"), // Obtendo o emissor.
            SigningCredentials = signingCredentials // Definindo as credenciais de assinatura que eu gerei acima.
        };

        // Aqui eu crio um manipulador do token JWT que é responsável por criar e válidar os tokens.
        var tokenHandler = new JwtSecurityTokenHandler();

        // Crio o token com as informações do meu tokenDescriptor criado acima.
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        // Crio uma variável com um array de bytes com o tamanho de 128 bytes, que vai armazenar bytes aleatórios que serão gerados de forma segura.
        var secureRandomBytes = new byte[128];

        // Crio um gerador de números aleatórios.
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(secureRandomBytes); // Preencho o meu array de bytes com os bytes gerados aleatóriamente.

        // Converto os bytes aleatórios para uma representação de string no formato Base64.
        var refreshToken = Convert.ToBase64String(secureRandomBytes);
        return refreshToken;
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration _config)
    {
        // Inicio obtendo a minha chave secreta.
        var secretKey = _config["JWT:SecretKey"]
            ?? throw new InvalidOperationException("Invalid key");

        // Definindo os parâmetros de validação para o token expirado.
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        // Criando um manipulador do token JWT.
        var tokenHandler = new JwtSecurityTokenHandler();

        // Validando o token
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken); // O out SecurityToken securityToken vai ser preenchido com as informações obtidas do tokan.

        // Aqui eu verifico se o meu securityToken é uma instância de JwtSecurityToken e se o algoritmo utilizado é o HmacSha256
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
