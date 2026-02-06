namespace ApiChamados.Application.DTO
{
    public class ArquivoReclamacaoDto
    {
        public byte[] Conteudo { get; set; }
        public string NomeArquivo { get; set; }
        public string Bucket { get; set; }

        public ArquivoReclamacaoDto(byte[] conteudo, string nomeArquivo, string bucket)
        {
            Conteudo = conteudo;
            NomeArquivo = nomeArquivo;
            Bucket = bucket;
        }
    }
}
