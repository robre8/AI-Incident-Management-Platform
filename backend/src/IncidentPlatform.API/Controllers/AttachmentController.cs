using IncidentPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.API.Controllers;

[ApiController]
[Route("api/incidents/{incidentId:guid}/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAwsFileService _fileService;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".txt", ".csv", ".log",
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".json", ".xml", ".yaml", ".yml"
    };

    public AttachmentController(IAwsFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(Guid incidentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        if (file.Length > MaxFileSize)
            return BadRequest($"File exceeds the {MaxFileSize / (1024 * 1024)} MB limit.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return BadRequest($"File type '{extension}' is not allowed.");

        using var stream = file.OpenReadStream();

        var key = await _fileService.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType
        );

        return Ok(new
        {
            IncidentId = incidentId,
            FileKey = key
        });
    }
}
