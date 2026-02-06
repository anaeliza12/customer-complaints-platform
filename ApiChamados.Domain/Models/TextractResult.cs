namespace ApiChamados.Domain.Models
{
    public class TextractResult
    {
        public string Canal { get; set; }
        public string Protocolo { get; set; }
        public DateTime DataCriacaoUsuario { get; set; }
        public Reclamante Reclamante { get; set; }
        public Empresa EmpresaReclamada { get; set; }
        public string DescricaoReclamacao { get; set; }
        public string Pedido { get; set; }
        public double ConfiancaMedia { get; set; }
    }

    public class Reclamante
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
    }

    public class Empresa
    {
        public string Banco { get; set; }
        public string Cnpj { get; set; }
    }
}
