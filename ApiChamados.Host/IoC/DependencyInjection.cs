using Amazon;
using Amazon.EventBridge;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using ApiChamados.Adapters.AI;
using ApiChamados.Adapters.Config;
using ApiChamados.Adapters.Data.DataMesh;
using ApiChamados.Adapters.Data.dbContext;
using ApiChamados.Adapters.Repositories;
using ApiChamados.Adapters.Services;
using ApiChamados.Adapters.Storage;
using ApiChamados.Application.DTO;
using ApiChamados.Application.Ports.AI;
using ApiChamados.Application.Ports.Data;
using ApiChamados.Application.Ports.Messaging;
using ApiChamados.Application.Ports.Repositories;
using ApiChamados.Application.Ports.Storage;
using ApiChamados.Application.Ports.Worker;
using ApiChamados.Domain.Services;
using ApiChamados.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiChamados.Host.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ProcessarReclamacaoUseCase>();
            services.AddSingleton<ClassificadorReclamacao>();

            return services;
        }

        public static IServiceCollection AddAdapters(
            this IServiceCollection services,
            IConfiguration configuration,
            AWSCredentials credentials,
            RegionEndpoint region,
            string bucketName)
        {
            #region AWS
            services.AddSingleton<IAmazonSQS>(new AmazonSQSClient(credentials, region));
            services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, region));
            services.AddAWSService<IAmazonEventBridge>();
            #endregion

            #region Messaging
            services.AddScoped<IEventPublisher, EventBridgePublisher>();
            #endregion Messaging

            #region Storage
            services.AddScoped<IStorageService>(sp =>
                new StorageService(sp.GetRequiredService<IAmazonS3>(), bucketName));
            #endregion

            #region AI
            services.AddScoped<ITextractService, TextractAdapter>();
            #endregion

            #region DB
            var conn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));
            services.AddScoped<IReclamacaoRepository, ReclamacaoRepository>();
            #endregion

            #region DataMesh
            services.Configure<Endpoints>(configuration.GetSection("Endpoints"));
            services.AddHttpClient<IDataMeshService, DataMeshAdapter>((sp, http) =>
            {
                var endpoints = sp.GetRequiredService<IOptions<Endpoints>>().Value;
                http.BaseAddress = new Uri(endpoints.DataMesh.BaseUrl);
            });
            #endregion

            return services;
        }

        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            services.AddScoped<ISqsMessageHandler<S3SQSEventDto>, S3MessageHandler>();
            services.AddScoped<ISqsMessageHandler<CanalExternoDto>, CanalExternoMessageHandler>();

            services.AddHostedService<SqsWorkerExterno>();
            services.AddHostedService<SqsWorkerS3>();

            return services;
        }
    }
}

