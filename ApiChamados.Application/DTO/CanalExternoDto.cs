namespace ApiChamados.Application.DTO
{
    public class CanalExternoDto
    {
        public string Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public ReclamanteDto Cliente { get; set; }
        public string Texto { get; set; }
        public List<AnexoDto> Anexos { get; set; }
    }

    public class ReclamanteDto
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
    }

    public class AnexoDto
    {
        public string NomeArquivo { get; set; }
        public string Tipo { get; set; }
        public byte[] Base64 { get; set; }
    }
}
