using ApiChamados.Application.DTO;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CanalExternoMessageHandler : ISqsMessageHandler<CanalExternoDto>
{
    private readonly ProcessarReclamacaoUseCase _useCase;
    private readonly ILogger<CanalExternoMessageHandler> _logger;

    public CanalExternoMessageHandler(
        ProcessarReclamacaoUseCase useCase,
        ILogger<CanalExternoMessageHandler> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public async Task HandleAsync(string messageBody, string messageId)
    {
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", messageId))
        {
            _logger.LogInformation("Mensagem recebida do Canal Externo. Iniciando processamento...");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var canalExterno = JsonSerializer.Deserialize<CanalExternoDto>(messageBody, options);

            if (canalExterno == null)
            {
                _logger.LogWarning("Mensagem do canal externo inválida.");
                return;
            }

            await _useCase.ProcessarMensagemAsyncExterno(canalExterno, messageId);

            _logger.LogInformation("Processamento do Canal Externo finalizado com sucesso.");
        }
    }
}
