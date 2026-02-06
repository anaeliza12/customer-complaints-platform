namespace ApiChamados.Application.Ports.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T detail, string source, string detailType) where T : class;
        Task PublishNovaReclamacaoAsync(object reclamacao);
    }
}