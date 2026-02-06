using ApiChamados.Adapters.Data.Entities;
using ApiChamados.Domain.Models;

namespace ApiChamados.Adapters.Mappers
{
    public static class ReclamacaoMapper
    {
        public static ReclamacaoDbModel ToDbModel(this Reclamacao domain)
        {
            if (domain == null) return null;

            return new ReclamacaoDbModel
            {
                CorrelationId = domain.CorrelationId,
                Texto = domain.Texto,
                SLAViolado = false,
                UltimaNotificacaoSLA = domain.UltimaNotificacaoSLA,
                DataCriacao = domain.DataCriacao,
                DataRecebimento = domain.DataRecebimento,
                Status = domain.Status.ToString(),
                Cliente = domain.Cliente != null ? new ClienteDbModel
                {
                    Nome = domain.Cliente.Nome,
                    Cpf = domain.Cliente.Cpf,
                    Email = domain.Cliente.Email 
                } : null,
                PerfilCliente = domain.PerfilCliente != null ? new PerfilClienteDbModel
                {
                    Cpf = domain.Cliente?.Cpf,
                    ScoreCredito = domain.PerfilCliente.ScoreCredito, 
                    Produtos = domain.PerfilCliente.Produtos, 
                    RelacionamentoAnos = domain.PerfilCliente.RelacionamentoAnos,
                    Risco = domain.PerfilCliente.Risco          
                } : null,
                Anexos = domain.Anexos?.Select(a => new AnexoDbModel
                {
                    NomeArquivo = a.NomeArquivo,
                    Key = a.Key
                }).ToList(),
                Categorias = domain.Categorias?.Select(cat => new ReclamacaoCategoriaDbModel
                {
                    Nome = cat
                }).ToList(),
            };
        }
    }
}