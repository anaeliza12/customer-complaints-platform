using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using ApiChamados.Application.UseCases;
using ApiChamados.Application.DTO;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ApiChamados.Ports.Worker
{
    public class SqsWorkerS3 : BackgroundService
    {
        private readonly ProcessarReclamacaoUseCase _useCase;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public SqsWorkerS3(ProcessarReclamacaoUseCase useCase)
        {
            _useCase = useCase;

            Env.Load();

            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");
            var queueName = Environment.GetEnvironmentVariable("SQS_QUEUE_NAME_DOC");

            _sqsClient = new AmazonSQSClient(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(region)
            );

            _queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL_DOC");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker iniciado. Escutando SQS channel...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10,
                    VisibilityTimeout = 30
                });

                Console.WriteLine(response.GetType().Assembly.FullName);

                if (response.Messages == null)
                {
                    continue;
                }

                foreach (var msg in response.Messages)
                {
                    try
                    {
                        var doc = JsonDocument.Parse(msg.Body);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var s3Event = JsonSerializer.Deserialize<S3SQSEventDto>(doc.RootElement.GetRawText(), options);

                        if (s3Event == null)
                        {
                            Console.WriteLine("Evento S3 nulo.");
                            continue;
                        }

                        var records = s3Event.Records;

                        foreach (var record in records)
                        {
                            await _useCase.ProcessarMensagemAsyncDoc(record);
                        }

                        //await _sqsClient.DeleteMessageAsync(_queueUrl, msg.ReceiptHandle);

                        Console.WriteLine("Mensagem deletada da fila.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro: {ex.Message}");
                    }
                }
            }
        }
    }
}