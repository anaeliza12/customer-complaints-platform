using ApiChamados.Application.DTO;
using ApiChamados.Application.Ports.AI;
using ApiChamados.Application.Ports.Data;
using ApiChamados.Application.Ports.Repositories;
using ApiChamados.Application.Ports.Storage; // Adicionado para o IStorageService
using ApiChamados.Application.UseCases;
using ApiChamados.Domain.Models;
using ApiChamados.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit; // Certifique-se de ter o xunit instalado

namespace ApiChamados.Tests.Application.UseCases
{
    public class ProcessarReclamacaoUseCaseTests
    {
        private readonly Mock<ITextractService> _textractMock;
        private readonly Mock<IReclamacaoRepository> _repoMock;
        private readonly Mock<IDataMeshService> _datameshMock; // Substituiu o DataMesh
        private readonly Mock<IStorageService> _storageMock; // Substituiu o DataMesh
        private readonly Mock<ILogger<ProcessarReclamacaoUseCase>> _loggerMock;
        private readonly ClassificadorReclamacao _classificador;
        private readonly ProcessarReclamacaoUseCase _useCase;

        public ProcessarReclamacaoUseCaseTests()
        {
            _textractMock = new Mock<ITextractService>();
            _repoMock = new Mock<IReclamacaoRepository>();
            _storageMock = new Mock<IStorageService>(); // Inicializado
            _datameshMock = new Mock<IDataMeshService>(); // Inicializado
            _loggerMock = new Mock<ILogger<ProcessarReclamacaoUseCase>>();

            // Mock do logger para o classificador
            var classificadorLoggerMock = new Mock<ILogger<ClassificadorReclamacao>>();
            _classificador = new ClassificadorReclamacao(classificadorLoggerMock.Object);

            // A ordem deve respeitar o Primary Constructor do UseCase:
            // 1. ITextractService
            // 2. IReclamacaoRepository
            // 3. IStorageService
            // 4. ClassificadorReclamacao
            // 5. ILogger
            _useCase = new ProcessarReclamacaoUseCase(
                _textractMock.Object,
                _repoMock.Object,
                _storageMock.Object, // Injetado aqui
                _classificador,
                _datameshMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessarMensagemAsyncDoc_DeveInterromper_QuandoIdempotenciaAcionada()
        {
            // Arrange
            string correlationId = "doc_ja_processado_123";
            var s3Event = new S3Records
            {
                EventName = "ObjectCreated:Put",
                S3 = new S3EntityDto
                {
                    Bucket = new S3BucketDto { Name = "bucket-teste" },
                    Object = new S3ObjectDto { Key = "reclamacao.pdf" }
                }
            };

            _repoMock.Setup(r => r.ExistePorCorrelationIdAsync(correlationId))
                     .ReturnsAsync(true);

            // Act
            await _useCase.ProcessarMensagemAsyncDoc(s3Event, correlationId);

            // Assert
            _textractMock.Verify(t => t.AnalisarDocumentoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IdempotenciaAcionada")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemAsyncExterno_DeveCalcularSLAComoViolado_QuandoDataForAntiga()
        {
            // Arrange
            string correlationId = "id_novo";
            var dto = new CanalExternoDto
            {
                DataCriacao = DateTime.UtcNow.AddDays(-15), // Criado há 15 dias
                Cliente = new ReclamanteDto { Nome = "João", Cpf = "123", Email = "j@j.com" },
                Texto = "Problema no app",
                Anexos = new List<AnexoDto>() // Evita null reference se a lógica iterar
            };

            _repoMock.Setup(r => r.ExistePorCorrelationIdAsync(correlationId)).ReturnsAsync(false);

            // Act
            await _useCase.ProcessarMensagemAsyncExterno(dto, correlationId);

            // Assert
            _repoMock.Verify(r => r.SalvarAsync(It.Is<Reclamacao>(rec => rec.SLAViolado == true)), Times.Once);
        }

        [Fact]
        public async Task ProcessarMensagemAsyncDoc_DeveChamarIAESalvar_QuandoMensagemForNova()
        {
            // Arrange
            string correlationId = "novo_doc";
            var s3Event = new S3Records
            {
                S3 = new S3EntityDto
                {
                    Bucket = new S3BucketDto { Name = "bucket-teste" },
                    Object = new S3ObjectDto { Key = "arquivo.pdf" }
                }
            };

            _repoMock.Setup(r => r.ExistePorCorrelationIdAsync(correlationId)).ReturnsAsync(false);
            _textractMock.Setup(t => t.AnalisarDocumentoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TextractResult
                {
                    Reclamante = new Reclamante { Nome = "Teste", Cpf = "123", Email = "t@t.com" },
                    DescricaoReclamacao = "Reclamação de boleto",
                    DataCriacaoUsuario = DateTime.UtcNow,
                    Canal = "Fisico"
                });

            // Act
            await _useCase.ProcessarMensagemAsyncDoc(s3Event, correlationId);

            // Assert
            _textractMock.Verify(t => t.AnalisarDocumentoAsync("bucket-teste", "arquivo.pdf"), Times.Once);
            _repoMock.Verify(r => r.SalvarAsync(It.IsAny<Reclamacao>()), Times.Once);
        }
    }
}