using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using ApiChamados.Application.Ports.Messaging;
using System.Text.Json;

namespace ApiChamados.Infrastructure.Messaging
{
    public class EventBridgePublisher : IEventPublisher
    {
        private readonly IAmazonEventBridge _ebClient;

        public EventBridgePublisher(IAmazonEventBridge ebClient)
        {
            _ebClient = ebClient;
        }

        public async Task PublishNovaReclamacaoAsync(object reclamacao)
        {
            await PublishAsync(reclamacao, "api.chamados", "NovaReclamacao");
        }

        public async Task PublishAsync<T>(T detail, string source, string detailType) where T : class
        {
            var request = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        Source = source,
                        DetailType = detailType,
                        Detail = JsonSerializer.Serialize(detail),
                        EventBusName = "default"
                    }
                }
            };
            await _ebClient.PutEventsAsync(request);
        }
    }
}