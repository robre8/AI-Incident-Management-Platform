using IncidentPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IncidentPlatform.API.Controllers;

[ApiController]
[Route("api/incidents/{incidentId:guid}/attachments")]
public class AttachmentController : ControllerBase
{
    private readonly IAwsFileService _fileService;

    public AttachmentController(IAwsFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(Guid incidentId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

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
