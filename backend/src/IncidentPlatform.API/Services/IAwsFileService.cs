namespace IncidentPlatform.API.Services;

public interface IAwsFileService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
}
