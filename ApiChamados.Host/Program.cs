using Amazon;
using Amazon.Runtime;
using AWS.Logger;
using AWS.Logger.SeriLog;
using DotNetEnv;
using Microsoft.Extensions.Hosting;
using Serilog;
using ApiChamados.Host.IoC;

Env.Load();

var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
var regionName = Environment.GetEnvironmentVariable("AWS_REGION");
var regionEndpoint = RegionEndpoint.GetBySystemName(regionName);
var s3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME");

var awsCredentials = new BasicAWSCredentials(accessKey, secretKey);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.AWSSeriLog(new AWSLoggerConfig("ApiChamados/WorkerLogs")
    {
        Region = regionName,
        Credentials = awsCredentials
    })
    .CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args);

    builder.UseSerilog();

    builder.ConfigureServices((context, services) =>
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddApplication();
        services.AddAdapters(context.Configuration, awsCredentials, regionEndpoint, s3BucketName);
        services.AddWorkers();
    });

    await builder.Build().RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
