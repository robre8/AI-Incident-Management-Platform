using System.ComponentModel.DataAnnotations;

namespace IncidentPlatform.Domain.Entities;

public class IncidentDTO
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "Open";
}
