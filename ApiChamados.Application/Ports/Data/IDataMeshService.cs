using ApiChamados.Domain.Models;

namespace ApiChamados.Application.Ports.Data
{
    public interface IDataMeshService
    {
        Task<PerfilCliente> ObterPerfilClienteAsync(string cpf);
    }
}
