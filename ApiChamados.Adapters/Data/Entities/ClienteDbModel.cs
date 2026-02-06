namespace ApiChamados.Adapters.Data.Entities
{
    public class ClienteDbModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public ICollection<ReclamacaoDbModel> Reclamacoes { get; set; }
    }
}