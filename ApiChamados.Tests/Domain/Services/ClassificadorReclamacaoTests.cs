using ApiChamados.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiChamados.Tests.Domain.Services
{
    public class ClassificadorReclamacaoTests
    {
        private readonly Mock<ILogger<ClassificadorReclamacao>> _loggerMock;
        private readonly ClassificadorReclamacao _classificador;

        public ClassificadorReclamacaoTests()
        {
            _loggerMock = new Mock<ILogger<ClassificadorReclamacao>>();
            _classificador = new ClassificadorReclamacao(_loggerMock.Object);
        }

        [Theory]
        [InlineData("O APPLICATIVO está travando", "aplicativo")]
        [InlineData("Problema no meu SÉGURO de vida", "seguros")]
        [InlineData("Quero fazer o LOGIN e trocar a SENHA", "acesso")]
        public void Classificar_DeveIdentificarCategoria_QuandoTextoContemPalavrasChave(string texto, string categoriaEsperada)
        {
            // Act
            var resultado = _classificador.Classificar(texto);

            // Assert
            Assert.Contains(categoriaEsperada, resultado);
        }

        [Fact]
        public void Classificar_DeveRetornarMultiplasCategorias_QuandoTextoEhAmbiguo()
        {
            // Texto que toca em Fraude e Aplicativo
            string texto = "Houve uma FRAUDE no meu cartão e o APP não abre.";

            var resultado = _classificador.Classificar(texto);

            Assert.Contains("fraude", resultado);
            Assert.Contains("aplicativo", resultado);
        }

        [Fact]
        public void Classificar_DeveRetornarVazio_QuandoTextoForNuloOuEspaco()
        {
            var resultado = _classificador.Classificar("   ");

            Assert.Empty(resultado);
            // Verifica se o LogWarning foi chamado
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("vazio")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}