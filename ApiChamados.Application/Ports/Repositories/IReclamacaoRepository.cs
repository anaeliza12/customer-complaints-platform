using ApiChamados.Domain.Models;

namespace ApiChamados.Application.Ports.Repositories
{
    public interface IReclamacaoRepository
    {
        Task<Reclamacao> SalvarAsync(Reclamacao reclamacao);
        Task<bool> ExistePorCorrelationIdAsync(string correlationId);
    }
}
