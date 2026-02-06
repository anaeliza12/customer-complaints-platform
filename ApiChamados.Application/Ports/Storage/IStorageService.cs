namespace ApiChamados.Application.Ports.Storage
{
    public interface IStorageService
    {
        Task<string> SalvarArquivoAsync(string caminho, byte[] conteudo);
    }
}
