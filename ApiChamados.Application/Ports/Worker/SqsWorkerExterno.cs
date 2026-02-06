using Amazon.SQS;
using Amazon.SQS.Model;
using ApiChamados.Application.DTO;
using ApiChamados.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ApiChamados.Application.Ports.Worker
{
    public class SqsWorkerExterno(
        ILogger<SqsWorkerExterno> logger,
        IAmazonSQS sqsClient,
        IServiceScopeFactory scopeFactory
    ) : BackgroundService
    {
        private readonly string _queueUrl =
            Environment.GetEnvironmentVariable("SQS_QUEUE_URL_CHANNEL") ?? string.Empty;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation(
                "Metrica:WorkerStarted | Worker: SqsWorkerExterno | QueueUrl: {QueueUrl}",
                _queueUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var swPolling = Stopwatch.StartNew();

                    var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 10,
                        VisibilityTimeout = 120
                    }, stoppingToken);

                    swPolling.Stop();

                    logger.LogInformation(
                        "Metrica:SqsPolling | Worker: SqsWorkerExterno | TempoMS: {Tempo}",
                        swPolling.ElapsedMilliseconds);

                    if (response.Messages == null || response.Messages.Count == 0)
                    {
                        logger.LogDebug(
                            "Metrica:SqsEmptyPoll | Worker: SqsWorkerExterno");
                        continue;
                    }

                    logger.LogInformation(
                        "Metrica:SqsMessagesReceived | Canal: Digital | Quantidade: {Qtd}",
                        response.Messages.Count);

                    foreach (var msg in response.Messages)
                    {
                        using var scope = scopeFactory.CreateScope();
                        var handler = scope.ServiceProvider
                            .GetRequiredService<ISqsMessageHandler<CanalExternoDto>>();

                        try
                        {
                            logger.LogInformation(
                                "Metrica:MensagemRecebida | Canal: Digital | MessageId: {MessageId}",
                                msg.MessageId);

                            await handler.HandleAsync(msg.Body, msg.MessageId);

                            await sqsClient.DeleteMessageAsync(
                                _queueUrl,
                                msg.ReceiptHandle,
                                stoppingToken);

                            logger.LogInformation(
                                "Metrica:MensagemProcessadaComSucesso | Canal: Digital | MessageId: {MessageId}",
                                msg.MessageId);
                        }
                        catch (ValidationException ex)
                        {
                            logger.LogWarning(
                                "Metrica:MensagemInvalidaDescartada | Canal: Digital | MessageId: {MessageId} | Erro: {Erro}",
                                msg.MessageId,
                                ex.Message);

                            await sqsClient.DeleteMessageAsync(
                                _queueUrl,
                                msg.ReceiptHandle,
                                stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Metrica:ErroProcessamentoMensagem | Canal: Digital | MessageId: {MessageId}",
                                msg.MessageId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Metrica:ErroConsultaFila | Worker: SqsWorkerExterno");

                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}
