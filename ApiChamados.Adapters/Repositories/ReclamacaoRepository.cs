using ApiChamados.Adapters.Data.dbContext;
using ApiChamados.Adapters.Mappers;
using ApiChamados.Application.Ports.Repositories;
using ApiChamados.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiChamados.Adapters.Repositories
{
    public class ReclamacaoRepository : IReclamacaoRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReclamacaoRepository> _logger;

        public ReclamacaoRepository(AppDbContext context, ILogger<ReclamacaoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Reclamacao> SalvarAsync(Reclamacao reclamacao)
        {
            try
            {
                var dbModel = reclamacao.ToDbModel();

                await _context.Reclamacoes.AddAsync(dbModel);
                await _context.SaveChangesAsync(); 

                _logger.LogInformation("Reclamação salva com sucesso. ID: {Id}", dbModel.Id);

                return reclamacao; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar Reclamacao.");
                throw;
            }
        }

        public async Task<bool> ExistePorCorrelationIdAsync(string correlationId)
        {
            _logger.LogDebug("Verificando duplicidade para CorrelationId: {CorrelationId}", correlationId);

            return await _context.Reclamacoes
                .AnyAsync(r => r.CorrelationId == correlationId);
        }
    }
}