namespace ApiChamados.Application.Exceptions
{
    public class IntegrationException(string serviceName, string message)
        : Exception($"Erro na integração com {serviceName}: {message}");
}

