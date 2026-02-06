using ApiChamados.Domain.Models;

namespace ApiChamados.Ports.Data
{
    public interface IDataMeshService
    {
        Task<DataMeshProfile> ObterPerfilClienteAsync(string cpf);
    }
}
