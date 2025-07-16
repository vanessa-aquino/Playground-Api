using APICatalog.Data;
using APICatalog.DTOs.Mappings;
using APICatalog.Interfaces;
using APICatalog.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APICatalogxUnitTests.UnitTests;

public class ProductsUnitTestController
{
    public IUnitOfWork repository;
    public IMapper mapper;

    // São marcadas como static pois são membros de classe, não dependem de instâncias específicas:
    public static DbContextOptions<AppDbContext> dbContextOptions { get; }
    public static string connectionString = "Server=localhost;DataBase=CatalogDB;Uid=developer;pwd=v37656306A*";

    // Definindo a instância do meu contexto para acessar o MySql:
    static ProductsUnitTestController()
    {
        dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;
    }


    public ProductsUnitTestController()
    {
        // Configurando o autoMapper para mapear os objetos da classe ProductDto para Product:
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProductDTOMappingProfile());
        });

        mapper = config.CreateMapper(); // Invoco o CreateMapper.
        var context = new AppDbContext(dbContextOptions); // Crio a instância do meu contexto com as opções.
        repository = new UnitOfWork(context); // Crio uma instância do meu UnitOfWork passando o meu contexto
    }
}
