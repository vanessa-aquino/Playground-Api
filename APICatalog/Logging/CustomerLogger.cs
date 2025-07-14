namespace APICatalog.Logging;

public class CustomerLogger : ILogger // Essa interface define os métodos necessários para registrar as mensagens de log.
{
    readonly string loggerName;
    readonly CustomLoggerProviderConfiguration loggerConfig;

    public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
    {
        loggerName = name; // Recebe o nome da categoria.
        loggerConfig = config; // Recebe uma instância de ProviderConfiguration
    }

    // Vai verificar se o nível de log desejado está habilitado com base na configuração.
    // Se não estiver habilitado, as mensagens de log desse nível não serão registradas.
    public bool IsEnabled(LogLevel logLevel) => logLevel == loggerConfig.LogLevel;

    // É o método que permite criar um escopo para mensagens de log. 
    public IDisposable BeginScope<TState>(TState state) => null; // Aqui eu retorno null pq não quero a implementação de logging personalizado, mas sou obrigada a definir essa assinatura.

    // Método chamado para registrar uma mensagem de log. Ele vai verificar se o nível de logging é permitido, e se for o caso, vai formatar a mensagem e escrever a mensagem no arquivo de log.
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                            Exception exception, Func<TState, Exception, string> formatter)
    {
        string message = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}";
        WriteTextToFile(message);
    }

    private void WriteTextToFile(string message)
    {
        string logfilePath = @"c:\temp\vanessa.txt";
        using (StreamWriter streamWriter = new StreamWriter(logfilePath, true))
        {
            try
            {
                streamWriter.WriteLine(message);
                streamWriter.Close();
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
