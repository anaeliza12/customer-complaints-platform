using ApiChamados.Domain.Enums;

namespace ApiChamados.Domain.Models
{
    public class Reclamacao
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public Cliente Cliente { get; set; }
        public string Texto { get; set; }
        public bool SLAViolado { get; set; }
        public DateTime UltimaNotificacaoSLA { get; set; }
        public DateTime DataCriacao { get; set; } 
        public DateTime DataRecebimento { get; set; }
        public StatusReclamacao Status { get; set; }
        public PerfilCliente PerfilCliente { get; set; }
        public List<Anexo> Anexos { get; set; } = new();
        public List<string> Categorias { get; set; }
    }
}
