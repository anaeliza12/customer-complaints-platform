namespace ApiChamados.Application.DTO
{
    public class S3SQSEventDto
    {
        public List<S3Records> Records { get; set; }
    }

    public class S3Records
    {
        public string EventVersion { get; set; }
        public string EventSource { get; set; }
        public string AwsRegion { get; set; }
        public string EventName { get; set; }
        public S3EntityDto S3 { get; set; }
    }

    public class S3EntityDto
    {
        public S3BucketDto Bucket { get; set; }
        public S3ObjectDto Object { get; set; }
    }

    public class S3BucketDto
    {
        public string Name { get; set; }
        public OwnerIdentityDto OwnerIdentity { get; set; }
        public string Arn { get; set; }
    }

    public class OwnerIdentityDto
    {
        public string PrincipalId { get; set; }
    }

    public class S3ObjectDto
    {
        public string Key { get; set; }
        public long Size { get; set; }
        public string ETag { get; set; }
        public string Sequencer { get; set; }
    }

}