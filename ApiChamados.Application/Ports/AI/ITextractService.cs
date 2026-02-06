using ApiChamados.Domain.Models;


namespace ApiChamados.Application.Ports.AI
{
    public interface ITextractService
    {
        Task<TextractResult> AnalisarDocumentoAsync(string bucket, string key);
    }
}
