using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using ApiChamados.Application.DTO;
using ApiChamados.Application.UseCases;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ApiChamados.Ports.Worker
{
    public class SqsWorkerExterno : BackgroundService
    {
        private readonly ProcessarReclamacaoUseCase _useCase;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl;

        public SqsWorkerExterno(ProcessarReclamacaoUseCase useCase)
        {
            _useCase = useCase;

            Env.Load();

            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION");
            var queueName = Environment.GetEnvironmentVariable("SQS_QUEUE_NAME_CHANNEL");

            _sqsClient = new AmazonSQSClient(
                accessKey,
                secretKey,
                RegionEndpoint.GetBySystemName(region)
            );

            _queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL_CHANNEL");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker iniciado. Escutando SQS doc...");

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10,
                    VisibilityTimeout = 120
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
                        Console.WriteLine($"ID: {msg.MessageId}, ReceiptHandle: {msg.ReceiptHandle}");


                        var canalExterno = JsonSerializer.Deserialize<CanalExternoDto>(doc.RootElement.GetRawText(), options);
                        await _useCase.ProcessarMensagemAsyncExterno(canalExterno);

                        var deleteResponse = await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                        {
                            QueueUrl = _queueUrl,
                            ReceiptHandle = msg.ReceiptHandle
                        });
                        Console.WriteLine($"Mensagem deletada? HTTP Status: {deleteResponse.HttpStatusCode}");
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