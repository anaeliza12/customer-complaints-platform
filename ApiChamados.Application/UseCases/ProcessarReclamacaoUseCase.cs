using ApiChamados.Application.DTO;
using ApiChamados.Application.Exceptions;
using ApiChamados.Application.Ports.AI;
using ApiChamados.Application.Ports.Data;
using ApiChamados.Application.Ports.Messaging;
using ApiChamados.Application.Ports.Repositories;
using ApiChamados.Application.Ports.Storage;
using ApiChamados.Application.Validators;
using ApiChamados.Domain.Models;
using ApiChamados.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ApiChamados.Domain.Enums;

public class ProcessarReclamacaoUseCase(
    ITextractService textract,
    IReclamacaoRepository repository,
    IStorageService storageService,
    ClassificadorReclamacao classificador,
    IDataMeshService dataMeshService,
    IEventPublisher eventPublisher,
    ILogger<ProcessarReclamacaoUseCase> logger)
{
    private readonly ITextractService _textract = textract;
    private readonly IReclamacaoRepository _repository = repository;
    private readonly IStorageService _storageService = storageService;
    private readonly ClassificadorReclamacao _classificador = classificador;
    private readonly IDataMeshService _dataMeshService = dataMeshService;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<ProcessarReclamacaoUseCase> _logger = logger;


    public async Task ProcessarMensagemAsyncDoc(S3Records record, string correlationId)
    {
        _logger.LogInformation("Metrica:InicioProcessamento | Canal: Fisico | {CorrelationId}", correlationId);

        if (await _repository.ExistePorCorrelationIdAsync(correlationId))
        {
            _logger.LogWarning("Idempotência acionada | Fisico | {CorrelationId}", correlationId);
            return;
        }

        var swIA = Stopwatch.StartNew();
        var resultado = await _textract.AnalisarDocumentoAsync(
            record.S3.Bucket.Name,
            record.S3.Object.Key);

        swIA.Stop();

        if (resultado == null)
            throw new IntegrationException("Amazon Textract", "Falha ao extrair dados.");

        _logger.LogInformation("TempoExtraçãoIA: {Ms}ms", swIA.ElapsedMilliseconds);

        var cliente = new Cliente
        {
            Nome = resultado.Reclamante.Nome,
            Cpf = resultado.Reclamante.Cpf,
            Email = resultado.Reclamante.Email
        };

        await ProcessarCore(
            correlationId,
            cliente,
            resultado.DescricaoReclamacao,
            resultado.DataCriacaoUsuario,
            anexos: null,
            canal: "Fisico");
    }


    public async Task ProcessarMensagemAsyncExterno(CanalExternoDto dto, string correlationId)
    {
        _logger.LogInformation("Metrica:InicioProcessamento | Canal: Digital | {CorrelationId}", correlationId);

        if (await _repository.ExistePorCorrelationIdAsync(correlationId))
        {
            _logger.LogWarning("Idempotência acionada | Digital | {CorrelationId}", correlationId);
            return;
        }

        var anexos = await ProcessarAnexos(dto, correlationId);

        var cliente = new Cliente
        {
            Nome = dto.Cliente.Nome,
            Cpf = dto.Cliente.Cpf,
            Email = dto.Cliente.Email
        };


        await ProcessarCore(
            correlationId,
            cliente,
            dto.Texto,
            dto.DataCriacao,
            anexos,
            canal: "Digital");
    }


    private async Task ProcessarCore(
        string correlationId,
        Cliente cliente,
        string texto,
        DateTime dataCriacao,
        List<Anexo>? anexos,
        string canal)
    {
        var swTotal = Stopwatch.StartNew();

        var perfilCliente = await _dataMeshService.ObterPerfilClienteAsync(cliente.Cpf);
        var categorias = _classificador.Classificar(texto);

        if (!categorias.Any())
            _logger.LogWarning("Categoria não identificada | {CorrelationId}", correlationId);

        var agora = DateTime.UtcNow;

        var reclamacao = new Reclamacao
        {
            CorrelationId = correlationId,
            Cliente = cliente,
            Texto = texto,
            DataCriacao = dataCriacao,
            Status = StatusReclamacao.EnviadaCanal,
            DataRecebimento = agora,
            SLAViolado = (agora - dataCriacao).TotalDays > 10,
            UltimaNotificacaoSLA = agora.Date,
            Anexos = anexos,
            PerfilCliente = perfilCliente,
            Categorias = categorias
        };

        var validator = new ReclamacaoValidator();
        var validationResult = await validator.ValidateAsync(reclamacao);

        if (!validationResult.IsValid)
        {
            var msgErro = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(msgErro);
        }

        await _eventPublisher.PublishNovaReclamacaoAsync(reclamacao);

        swTotal.Stop();
        _logger.LogInformation(
            "Metrica:SucessoProcessamento | Canal: {Canal} | TempoTotalMS: {Tempo}",
            canal,
            swTotal.ElapsedMilliseconds);
    }


    private async Task<List<Anexo>> ProcessarAnexos(CanalExternoDto dto, string correlationId)
    {
        var anexos = new List<Anexo>();

        if (dto.Anexos == null || !dto.Anexos.Any())
            return anexos;

        _logger.LogInformation("Processando {Qtd} anexos", dto.Anexos.Count);

        foreach (var anexoDto in dto.Anexos)
        {
            var sw = Stopwatch.StartNew();
            var path = $"externo/{correlationId}/{anexoDto.NomeArquivo}";

            try
            {
                await _storageService.SalvarArquivoAsync(path, anexoDto.Base64);
            }
            catch (Exception ex)
            {
                throw new IntegrationException("S3 Storage", ex.Message);
            }

            sw.Stop();

            _logger.LogInformation("Upload S3 OK | {Arquivo} | {Ms}ms", anexoDto.NomeArquivo, sw.ElapsedMilliseconds);

            anexos.Add(new Anexo
            {
                Key = path,
                NomeArquivo = anexoDto.NomeArquivo
            });
        }

        return anexos;
    }
}
