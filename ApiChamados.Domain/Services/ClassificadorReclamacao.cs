using Microsoft.Extensions.Logging;
using ApiChamados.Domain.Extensions;

namespace ApiChamados.Domain.Services
{
    public class ClassificadorReclamacao(ILogger<ClassificadorReclamacao> logger)
    {
        private readonly ILogger<ClassificadorReclamacao> _logger = logger;

        private readonly Dictionary<string, List<string>> _categoriasERegras = new()
        {
            { "imobiliário", new() { "credito imobiliario", "casa", "apartamento", "financiamento", "boleto" } },
            { "seguros", new() { "resgate", "capitalizacao", "socorro", "seguro" } },
            { "cobrança", new() { "fatura", "cobrança", "valor", "indevido", "juros", "boleto", "cobranca" } },
            { "acesso", new() { "acessar", "login", "senha", "autenticacao", "logar" } },
            { "aplicativo", new() { "app", "aplicativo", "travando", "erro", "lentidao" } },
            { "fraude", new() { "fatura", "nao reconhece divida", "fraude", "golpe", "contestacao", "divida" } }
        };

        public List<string> Classificar(string textoReclamacao)
        {
            if (string.IsNullOrWhiteSpace(textoReclamacao))
            {
                _logger.LogWarning("Texto da reclamação está vazio ou nulo. Não foi possível classificar.");
                return new List<string>();
            }

            _logger.LogInformation("Iniciando classificação do texto (Tamanho: {Tamanho} caracteres).", textoReclamacao.Length);

            var textoNormalizado = textoReclamacao.ToNormalizedSearchText();
            var categoriasEncontradas = new List<string>();

            foreach (var regra in _categoriasERegras)
            {
                var palavrasDetectadas = regra.Value
                    .Where(p => textoNormalizado.Contains(p.ToNormalizedSearchText()))
                    .ToList();

                if (palavrasDetectadas.Any())
                {
                    categoriasEncontradas.Add(regra.Key);
                    _logger.LogInformation("Categoria '{Categoria}' identificada pelas palavras-chave: {Palavras}",
                        regra.Key, string.Join(", ", palavrasDetectadas));
                }
            }

            if (!categoriasEncontradas.Any())
            {
                _logger.LogWarning("Caso ambíguo: Nenhuma categoria correspondente encontrada para o relato.");
            }
            else
            {
                _logger.LogInformation("Classificação finalizada com {Total} categorias encontradas.", categoriasEncontradas.Count);
            }

            return categoriasEncontradas.Distinct().ToList();
        }
    }
}