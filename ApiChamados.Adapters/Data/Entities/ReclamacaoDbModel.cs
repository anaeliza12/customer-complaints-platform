namespace ApiChamados.Adapters.Data.Entities
{
    public class ReclamacaoDbModel
    {
        public Guid Id { get; set; }
        public string CorrelationId { get; set; }
        public string Texto { get; set; }
        public bool SLAViolado { get; set; }
        public DateTime UltimaNotificacaoSLA { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataRecebimento { get; set; }
        public string Status { get; set; }
        public Guid ClienteId { get; set; }
        public ClienteDbModel Cliente { get; set; }
        public PerfilClienteDbModel PerfilCliente { get; set; }
        public List<AnexoDbModel> Anexos { get; set; } = new();
        public List<ReclamacaoCategoriaDbModel> Categorias { get; set; } = new();
    }
}