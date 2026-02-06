using Amazon.SQS;
using Amazon.SQS.Model;
using ApiChamados.Application.DTO;
using ApiChamados.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class SqsWorkerS3(
    ILogger<SqsWorkerS3> logger,
    IAmazonSQS sqsClient,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private readonly string _queueUrl =
        Environment.GetEnvironmentVariable("SQS_QUEUE_URL_DOC") ?? string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Metrica:WorkerStarted | Worker: SqsWorkerS3 | QueueUrl: {QueueUrl}",
            _queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            var swPolling = Stopwatch.StartNew();

            var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 10
            }, stoppingToken);

            swPolling.Stop();

            logger.LogInformation(
                "Metrica:SqsPolling | Worker: SqsWorkerS3 | TempoMS: {Tempo}",
                swPolling.ElapsedMilliseconds);

            if (response.Messages == null || response.Messages.Count == 0)
            {
                logger.LogDebug("Metrica:SqsEmptyPoll | Worker: SqsWorkerS3");
                continue;
            }

            logger.LogInformation(
                "Metrica:SqsMessagesReceived | Quantidade: {Qtd}",
                response.Messages.Count);

            foreach (var msg in response.Messages)
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<ISqsMessageHandler<S3SQSEventDto>>();

                try
                {
                    logger.LogInformation(
                        "Metrica:MensagemRecebida | Canal: S3 | MessageId: {MessageId}",
                        msg.MessageId);

                    await handler.HandleAsync(msg.Body, msg.MessageId);

                    await sqsClient.DeleteMessageAsync(
                        _queueUrl,
                        msg.ReceiptHandle,
                        stoppingToken);

                    logger.LogInformation(
                        "Metrica:MensagemProcessadaComSucesso | Canal: S3 | MessageId: {MessageId}",
                        msg.MessageId);
                }
                catch (ValidationException)
                {
                    logger.LogWarning(
                        "Metrica:MensagemInvalidaDescartada | Canal: S3 | MessageId: {MessageId}",
                        msg.MessageId);

                    await sqsClient.DeleteMessageAsync(
                        _queueUrl,
                        msg.ReceiptHandle,
                        stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Metrica:ErroProcessamentoMensagem | Canal: S3 | MessageId: {MessageId}",
                        msg.MessageId);
                }
            }
        }
    }
}
