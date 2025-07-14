namespace APICatalog.Logging;

public class CustomLoggerProviderConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning; // Define o nível mínimo de log a ser registrado, iniciando com o padrão Warning.
    public int EventId { get; set; } = 0; // Define o Id do evento log, com o padrão sendo zero.
}
