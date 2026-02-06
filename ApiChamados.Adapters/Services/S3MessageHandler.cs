using ApiChamados.Application.DTO;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiChamados.Domain.Extensions;

namespace ApiChamados.Adapters.Services
{
    public class S3MessageHandler : ISqsMessageHandler<S3SQSEventDto>
    {
        private readonly ProcessarReclamacaoUseCase _useCase;
        private readonly ILogger<S3MessageHandler> _logger;

        public S3MessageHandler(
            ProcessarReclamacaoUseCase useCase,
            ILogger<S3MessageHandler> logger)
        {
            _useCase = useCase;
            _logger = logger;
        }

        public async Task HandleAsync(string messageBody, string messageId)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            using var doc = JsonDocument.Parse(messageBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("Event", out var eventProp) &&
                eventProp.GetString() == "s3:TestEvent")
            {
                _logger.LogInformation("Evento de teste do S3 ignorado.");
                return;
            }

            var s3Event = JsonSerializer.Deserialize<S3SQSEventDto>(messageBody, options);
            if (s3Event?.Records == null) return;

            foreach (var record in s3Event.Records)
            {
                var correlationId = record.S3.Object.Key.ToCorrelationId();

                using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
                {
                    await _useCase.ProcessarMensagemAsyncDoc(record, correlationId);
                }
            }
        }
    }
}
