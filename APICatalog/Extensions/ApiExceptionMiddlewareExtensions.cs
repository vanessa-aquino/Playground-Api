using APICatalog.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace APICatalog.Extensions;

public static class ApiExceptionMiddlewareExtensions
{
    // Esse método vai ser adicionado ao objeto IApplicationBuilder, que é uma interface usada para configurar o pipeline de requisições http da nossa aplicação.
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        // Na primeira linha, estou configurando o middleware de tratamento de exceções, recebendo um delegate que será executado quando uma exceção não tratada ocorrer durante o processamento.
        app.UseExceptionHandler(appError =>
        {
            // Aqui dentro do delegate de "UseExceptionHandler", eu específico o que fazer quando ocorrer uma exceção não tratada, através do método "Run".
            // Então, aqui eu crio um contexto de resposta, representado por "context". 
            appError.Run(async context =>
            {
                // Aqui eu estou definindo um código de status da resposta http como 500, para indicar um erro interno do servidor. 
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // Aqui eu defino o tipo de conteúdo da resposta como Json, indicando que a resposta conterá dados no formato Json.
                context.Response.ContentType = "application/json";

                // Depois, obtemos uma caracteristica/feature do manipulador de exceções do contexto, para acessar informações sobre a exceção que ocorreu.
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                // Em seguida, nós verificamos se essa feature não é nula, ou seja, se ela de fato ocorreu durante o processamento do Request.
                if (contextFeature != null)
                {
                    // E aqui, nós escrevemos os detalhes da exceção no formato Json, na resposta Http. 
                    await context.Response.WriteAsync(new ErrorDetails()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message,
                        Trace = contextFeature.Error.StackTrace
                    }.ToString()); // Método ToString() para serializar as informações no formato Json.
                }
            });
        });
    }
}
