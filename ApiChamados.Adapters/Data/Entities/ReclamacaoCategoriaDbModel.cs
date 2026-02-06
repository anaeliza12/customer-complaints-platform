namespace ApiChamados.Adapters.Data.Entities
{
    public class ReclamacaoCategoriaDbModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; }
        public Guid ReclamacaoId { get; set; }
        public ReclamacaoDbModel Reclamacao { get; set; }
    }
}