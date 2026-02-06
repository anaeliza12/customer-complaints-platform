using ApiChamados.Application.Ports.AI;
using ApiChamados.Domain.Models;

namespace ApiChamados.Adapters.AI
{
    public class TextractAdapter : ITextractService
    {
        public Task<TextractResult> AnalisarDocumentoAsync(string bucket, string key)
        {
            if (key.Contains("PROCON", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new TextractResult
                {
                    Canal = "PROCON",
                    Protocolo = "PROCON-2026-009871",
                    DataCriacaoUsuario = new DateTime(2026, 1, 25),
                    Reclamante = new Reclamante
                    {
                        Nome = "João da Silva Pereira",
                        Cpf = "11111111111",
                        Email = "joao@email.com"
                    },
                    EmpresaReclamada = new Empresa
                    {
                        Banco = "Banco Itaú Unibanco S.A.",
                        Cnpj = "60701190000104"
                    },
                    DescricaoReclamacao = "Não consegue acessar o aplicativo do banco. Cobrança indevida no valor de R$ 980,00.",
                    Pedido = "Cancelamento da cobrança e análise de fraude",
                    ConfiancaMedia = 0.98
                });
            }

            return Task.FromResult(new TextractResult
            {
                Canal = "BACEN",
                Protocolo = "BACEN-2026-554433",
                DataCriacaoUsuario = new DateTime(2026, 1, 22),
                Reclamante = new Reclamante
                {
                    Nome = "Maria Fernanda Costa",
                    Cpf = "22222222222",
                    Email= "maria@gmail.com"
                },
                EmpresaReclamada = new Empresa
                {
                    Banco = "Banco Itaú Unibanco S.A.",
                    Cnpj = "60701190000104"
                },
                DescricaoReclamacao = "Cobrança de seguro não contratado. Erro ao acessar aplicativo.",
                Pedido = "Cancelamento e estorno",
                ConfiancaMedia = 0.97
            });
        }
    }
}
