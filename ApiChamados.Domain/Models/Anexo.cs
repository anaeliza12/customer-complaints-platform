namespace ApiChamados.Domain.Models
{
    public class Anexo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Key { get; set; }
        public string NomeArquivo { get; set; }
        public Guid ReclamacaoId { get; set; } 
    }
}