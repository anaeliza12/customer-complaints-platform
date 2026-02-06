using ApiChamados.Domain.Extensions;

namespace ApiChamados.Tests.Domain.Extensions
{
    public class StringExtensionsTests
    {
        #region Testes para ToNormalizedSearchText

        [Theory]
        [InlineData("Olá Mundo!", "ola mundo")]
        [InlineData("Atenção à Cobrança", "atencao a cobranca")]
        [InlineData("SÉGURO CAPITALIZAÇÃO", "seguro capitalizacao")]
        [InlineData("E-mail com caracteres @#$%!", "email com caracteres ")]
        [InlineData("   Texto com espaços   ", "texto com espacos")]
        [InlineData(null, "")]
        public void ToNormalizedSearchText_DeveNormalizarCorretamente(string input, string esperado)
        {
            var resultado = input.ToNormalizedSearchText();

            Assert.Equal(esperado, resultado);
        }

        #endregion

        #region Testes para ToCorrelationId

        [Theory]
        [InlineData("Manual do Usuario.pdf", "manual_do_usuario_pdf")]
        [InlineData("reclamacao_site_a_2026.json", "reclamacao_site_a_2026_json")]
        [InlineData("protocolo 123/456-A", "protocolo_123_456_a")]
        [InlineData("Arquivo%20com%20Espaco.jpg", "arquivo_com_espaco_jpg")] // Testa o UrlDecode
        [InlineData("---muitos---traços---", "muitos_tracos")] // Testa o Regex de substituição múltipla
        [InlineData("", "")]
        public void ToCorrelationId_DeveGerarIdLimpoESeguro(string input, string esperado)
        {
            var resultado = input.ToCorrelationId();

            Assert.Equal(esperado, resultado);
        }

        #endregion

        #region Testes de Casos de Borda (Edge Cases)

        [Fact]
        public void ToCorrelationId_QuandoInputPossuiAspas_DeveRemoverAntesDeProcessar()
        {
            string input = "arquivo_\"nome\"_v1.pdf";
            string esperado = "arquivo_nome_v1_pdf";

            var resultado = input.ToCorrelationId();

            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void ToNormalizedSearchText_QuandoInputForNumerico_DeveManterNumeros()
        {
            string input = "Protocolo 2026 12345";
            string esperado = "protocolo 2026 12345";

            var resultado = input.ToNormalizedSearchText();

            Assert.Equal(esperado, resultado);
        }

        #endregion
    }
}