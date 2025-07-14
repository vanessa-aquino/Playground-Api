using System.Collections.Concurrent;

namespace APICatalog.Logging;

public class CustomLoggerProvider : ILoggerProvider // Interface usada para criar instâncias de loggers personalizadas.
{
    readonly CustomLoggerProviderConfiguration loggerConfig;
    
    // A interface vai manter um dicionário de loggers, onde a chave é o nome da categoria, ou seja, uma string, e o valor, é uma instância de CustomerLogger.
    readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();

    public CustomLoggerProvider(CustomLoggerProviderConfiguration config)
    {
        // O construtor recebe uma instância do meu "CustomLoggerProviderConfiguration", que define a configuração para todos os loggers criados para esse provedor.
        loggerConfig = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Esse método vai ser usado para criar um logger para uma categoria específica. Ele vai retornar um logger existente do dicionário "loggers", ou se ainda não existir, ele vai criar um novo, se necessário.
        return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig));
    }

    // Aqui nesse método, eu estou liberando os recursos assim que meu método for descartado.
    public void Dispose() => loggers.Clear();
    
}
