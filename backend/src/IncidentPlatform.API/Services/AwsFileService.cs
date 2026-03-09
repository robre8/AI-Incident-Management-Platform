using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using IncidentPlatform.API.Models;

namespace IncidentPlatform.API.Services;

public class AwsFileService : IAwsFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsSettings _settings;

    public AwsFileService(IConfiguration configuration)
    {
        _settings = configuration.GetSection("AwsSettings").Get<AwsSettings>()!;

        var region = RegionEndpoint.GetBySystemName(_settings.Region);

        _s3Client = new AmazonS3Client(region);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var safeName = Path.GetFileName(fileName);
        var key = $"{Guid.NewGuid()}_{safeName}";

        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);

        return key;
    }
}
