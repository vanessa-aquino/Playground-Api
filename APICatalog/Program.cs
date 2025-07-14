using APICatalog.Data;
using APICatalog.DTOs.Mappings;
using APICatalog.Extensions;
using APICatalog.Filters;
using APICatalog.Interfaces;
using APICatalog.Logging;
using APICatalog.Models;
using APICatalog.RateLimitOptions;
using APICatalog.Repositories;
using APICatalog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }).AddNewtonsoftJson();

// Definindo a política de CORS:
var originsAllowedAccess = "_originsAllowedAccess";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: originsAllowedAccess,
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500") // Essa regra diz que só o front rodando em http://127.0.0.1:5500 pode fazer requisições pra sua API.
            .AllowAnyMethod() // Autorizo que o front user qualquer método http.
            .AllowAnyHeader(); // Autorizo qualquer cabeçalho HTTP vindo do front. 
        });
});


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "APICatalog", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() // Adicionando uma definição de segurança chamada "Bearer"
    {
        Name = "Authorization", // Nome do cabeçalho utilizado para enviar o token.
        Type = SecuritySchemeType.ApiKey, // Autenticação feita por meio de uma chave de api.
        Scheme = "Bearer", // Define o esquema de autenticação, é o portador do token.
        BearerFormat = "JWT", // Formato do token.
        In = ParameterLocation.Header, // Especifica que o token deve ser incluido no request, no header.
        Description = "Bearer JWT" // Descrição que vai aparecer na interface do Swagger.
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement // Defino que as operações da API, requerem o esquema de segurança Bearer, que defini acima.
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var MySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(MySqlConnectionString, ServerVersion.AutoDetect(MySqlConnectionString)));

// Configuração para usar o JWT Bearer:
var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key!");
builder.Services.AddAuthentication(options =>
{
    // Nessas duas linhas abaixo, eu defino que por padrão, o sistema de autenticação vai usar a autenticação baseada em Token JWT.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Se alguém tentar acessar um recurso protegido sem fornecer o token, irá solicitar as credenciais do usuario (fazer login).
}).AddJwtBearer(options =>
{
    options.SaveToken = true; // Salvar o token após uma autenticação bem sucedida.
    options.RequireHttpsMetadata = false; // É preciso https para transmitir o token? Em prod devo deixar com "true".
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Implementando a política de autorização:
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("GerenciaOnly", policy => policy.RequireRole("Admin").RequireClaim("id", "vanessa"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("User"))
    .AddPolicy("ExclusiveOnly", policy => policy.RequireAssertion(context =>
                      context.User.HasClaim(claim => claim.Type == "id" &&
                                                     claim.Value == "vanessa" ||
                                                     context.User.IsInRole("Gerencia"))));

// Vinculando os valores do arquivo de configuração com as propriedades definidas na classe.
var myOptions = new MyRateLimitOptions();
builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions); // O método Bind associa as configs obtidas na seção MyRateLimit à instância da classe MyRateLimitOptions.

// Implementando Rate Limiting
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(policyName: "fixedWindow", options =>
    {
        options.PermitLimit = myOptions.PermitLimit; // Quantidade máxima de requisições permitida por janela.
        options.Window = TimeSpan.FromSeconds(myOptions.Window); // Define o intervalo de tempo da janela fixa.
        options.QueueLimit = myOptions.QueueLimit; // Se alguém tentar fazer uma requisição além do limite não põe na fila, já bloqueia direto.
        // Se eu quisesse adicionar na lista de espera (ou seja, não me retorna erro, ele espera o meu timespan acabar para me entregar a requisição), ficaria assim:
        // options.QueueLimit = 2;
        // options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Definindo o esquema de versionamento da Api - Padrão QueryString e Padrão URI
builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0); // Define a versão padrão da API, geralmente 1.0.
    o.AssumeDefaultVersionWhenUnspecified = true; // Usa o valor definido em DefaultApiVersion caso o cliente não forneça uma versão explícita.
    o.ReportApiVersions = true; // Informa as versões de API suportadas nos headers de resposta.
    o.ApiVersionReader = ApiVersionReader.Combine( // Configura como ler a versão da API definida pelo cliente. O valor padrão é QueryString.
                         new QueryStringApiVersionReader(),
                         new UrlSegmentApiVersionReader());
}).AddApiExplorer(options => // É usado com o Swagger, corrige as rotas do endpoint e substitui o parâmetro de rota da versão da API.
{
    options.GroupNameFormat = "'v'VVV"; // Formatará a versão como "'v'major[.minor][-status]". Geralmente só é usado o "'v'major" mesmo.
    options.SubstituteApiVersionInUrl = true; // Só é necessária ao versionar usando o segmento URI.
});


builder.Services.AddScoped<ApiLoggingFilter>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Adicionando o provedor de log personalizado ao sistema de log do AspNet, definindo o nível mínimo de log como Information.
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    LogLevel = LogLevel.Information
}));

builder.Services.AddAutoMapper(typeof(ProductDTOMappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ConfigureExceptionHandler(); // Habilitando a utilização do meu método de extensão que vai usar os recursos do middleware.
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter(); // Habilitando o RateLimiter

app.UseCors(originsAllowedAccess); // Habilitando e dando suporte ao CORS e aplicando a minha política definida acima.

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
