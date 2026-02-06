namespace ApiChamados.Adapters.Data.Entities
{
    public class PerfilClienteDbModel
    {
        public string Cpf { get; set; }
        public int ScoreCredito { get; set; }
        public int RelacionamentoAnos { get; set; }
        public List<string> Produtos { get; set; }
        public string Risco { get; set; }
    }
}
