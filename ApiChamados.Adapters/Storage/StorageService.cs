using Amazon.S3;
using Amazon.S3.Transfer;
using ApiChamados.Application.Ports.Storage;

namespace ApiChamados.Adapters.Storage
{
    public class StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public StorageService(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        public async Task<string> SalvarArquivoAsync(string caminho, byte[] conteudo)
        {
            using var ms = new MemoryStream(conteudo);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = ms,
                Key = caminho,
                BucketName = _bucketName
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            return caminho;
        }
    }
}