using APICatalog.DTOs.Auth;
using APICatalog.Interfaces;
using APICatalog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalog.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager; // Serviço do identity que gerencia usuários.
    private readonly RoleManager<IdentityRole> _roleManager; // Serviço do identity que gerencia perfis/roles.
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [Route("CreateRole")]
    [Authorize(Policy = "GerenciaOnly")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        var roleExist = await _roleManager.RoleExistsAsync(roleName); // Verifica se a role já existe no banco.

        if (string.IsNullOrWhiteSpace(roleName))
            return BadRequest("Role name is required.");

        if (!roleExist)
        {
            if (_roleManager == null)
                throw new Exception("RoleManager não foi injetado corretamente.");

            // Cria uma nova role no banco com o nome informado.
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (roleResult.Succeeded) // Se a role foi criada com sucesso:
            {
                _logger.LogInformation(1, "Roles added");
                return StatusCode(200, new Response { Status = "Success", Message = $"Role {roleName} added successfully" });
            }
            else
            {
                _logger.LogInformation(2, "Error");
                return StatusCode(400, new Response { Status = "Error", Message = $"Issue adding the new {roleName} role" });
            }
        }
        return StatusCode(400, new Response { Status = "Error", Message = "Role already exist" });
    }

    [HttpPost]
    [Route("AddUserToRole")]
    [Authorize(Policy = "GerenciaOnly")]
    public async Task<IActionResult> AddUserToRole(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                return StatusCode(200, new Response { Status = "Success", Message = $"User {user.Email} added to the {roleName} role" });
            }
            else
            {
                _logger.LogInformation(1, $"Error: Unable to add user {user.Email} to the {roleName} role");
                return StatusCode(400, new Response { Status = "Error", Message = $"Error: unable to add user {user.Email} to the {roleName} role" });
            }
        }

        return BadRequest(new { error = "Unable to find user" });
    }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName!);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user); // Pega todos os roles desse usuário.

                var authClaims = new List<Claim> // Cria uma lista de claims
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("id", user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

                foreach (var userRole in userRoles)
                {
                    // Adiciona cara role do usuário como uma claim no token também.
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _configuration);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Lê do appsettings.Json quanto tempo o refresh token vai durar.
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

                // Salva no usuário:
                user.RefreshToken = refreshToken; // O token de refresh recém gerado.
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes); // Sua data de válidade.

                await _userManager.UpdateAsync(user); // Atualiza o usuario com o novo refresh token e a validade.

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token), // Acess token codificado
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo

                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName!);

            if (userExists != null)
                return StatusCode(409, new Response { Status = "Error", Message = "User already existis!" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
                return StatusCode(500, new Response { Status = "Error", Message = "User creation failed." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("refresh-token")] // É comum esse endpoint ser chamado automaticamente por apps quando o token JWT expira.
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel == null)
                return BadRequest("Invalid client request");

            // Garante que os dois tokens (access + refresh) foram enviados. Se não, lança exceção.
            string? accessToken = tokenModel.AccessToken ?? throw new ArgumentNullException(nameof(tokenModel));
            string? refreshToken = tokenModel.RefreshToken ?? throw new ArgumentException(nameof(tokenModel));

            // Mesmo com o access token expirado, o método GetPrincipalFromExpiredToken tenta extrair as claims (usuário, roles etc.) de dentro dele, usando a chave secreta original.
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _configuration);

            if (principal == null)
                return BadRequest("Invalid access token/refresh token");

            // Pega o nome de usuário que estava dentro do JWT.
            string userName = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(userName!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid access token/refresh token");

            // Gera um novo access token (JWT com as mesmas claims do anterior) e um novo refresh token aleatório.
            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Atualiza o usuário no banco com o novo refresh token.
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            // Retorna os novos tokens pro client num JSON.
            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Route("Revoke/{username}")]
        [Authorize(Policy = "ExclusiveOnly")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest("Invalid username");

            // Zera o refresh token salvo no banco — ou seja, revoga a sessão do usuário.
            // Isso invalida o token de "lembrar login". Ele terá que fazer login de novo.
            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }


    }
