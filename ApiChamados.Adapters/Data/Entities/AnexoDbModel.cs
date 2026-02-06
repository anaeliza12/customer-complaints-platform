namespace ApiChamados.Adapters.Data.Entities
{
    public class AnexoDbModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; }
        public string NomeArquivo { get; set; }
        public Guid ReclamacaoId { get; set; }
        public ReclamacaoDbModel Reclamacao { get; set; }
    }
}